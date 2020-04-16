using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HelmPreprocessor.Commands.Arguments;
using HelmPreprocessor.Configuration;
using HelmPreprocessor.Services.DeploymentRenderers;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Services
{
    public class DeploymentRendererContextBuilder : IDeploymentRendererContextBuilder
    {
        private readonly IDeploymentConfigurationProvider _deploymentConfigurationProvider;
        private readonly IDeploymentConfigurationPathProvider _deploymentConfigurationPathProvider;
        private readonly ISecretsHandler _secretsHandler;
        private readonly IOptions<ArgoCdEnvironment> _argoCdEnvironment;
        private readonly IOptions<RenderConfiguration> _renderConfiguration;
        private readonly IOptions<RenderArguments> _renderArguments;
        private readonly IOptions<GeneralArguments> _globalArguments;

        public DeploymentRendererContextBuilder(
            IDeploymentConfigurationProvider deploymentConfigurationProvider,
            IDeploymentConfigurationPathProvider deploymentConfigurationPathProvider,
            ISecretsHandler secretsHandler,
            IOptions<ArgoCdEnvironment> argoCdEnvironment,
            IOptions<RenderConfiguration> renderConfiguration,
            IOptions<RenderArguments> renderArguments,
            IOptions<GeneralArguments> globalArguments
            )
        {
            _deploymentConfigurationProvider = deploymentConfigurationProvider;
            _deploymentConfigurationPathProvider = deploymentConfigurationPathProvider;
            _secretsHandler = secretsHandler;
            _argoCdEnvironment = argoCdEnvironment;
            _renderConfiguration = renderConfiguration;
            _renderArguments = renderArguments;
            _globalArguments = globalArguments;
        }
        
        public DeploymentRendererContext GenerateDeploymentRendererContext()
        {
            if (!_deploymentConfigurationPathProvider.TryGetDeploymentConfigurationRoot(out var configurationRoot))
                throw new InvalidOperationException("Could not get configuration root directory for environment to render.");
            
            if (!_deploymentConfigurationProvider.GetDeploymentConfiguration(out var deploymentConfiguration))
                throw new InvalidOperationException("Could not build deployment configuration for environment to render.");
            
            var configurationRootDirectory = (DirectoryInfo) configurationRoot;

            return deploymentConfiguration!.Renderer.Type switch
            {
                RendererType.Helm2 => GenerateHelmDeploymentRendererContext(
                    configurationRootDirectory,
                    deploymentConfiguration
                ),
                RendererType.Helm3 => GenerateHelmDeploymentRendererContext(
                    configurationRootDirectory,
                    deploymentConfiguration
                ),
                _ => throw new InvalidOperationException("An unsupported deployment render was provided.")
            };
        }

        private DeploymentRendererContext GenerateHelmDeploymentRendererContext(
            DirectoryInfo configurationRootDirectory, 
            DeploymentConfiguration deploymentConfiguration
            )
        {
            // start building list of helm value files
            var helmValueFiles = new List<FileInfo>
            {
                new FileInfo(Path.Combine(configurationRootDirectory.FullName, "values.yaml")),
                new FileInfo(Path.Combine(configurationRootDirectory.FullName, Constants.APP_VERSIONS_FILENAME)),
                new FileInfo(Path.Combine(configurationRootDirectory.FullName, "secrets.yaml"))
            };
            
            foreach (var serviceMap in deploymentConfiguration.Services)
            {
                helmValueFiles.AddRange(
                    from s in new[] {"values.yaml", "secrets.yaml", "infra.yaml"}
                    select new FileInfo(Path.Combine(configurationRootDirectory.FullName, serviceMap.Key, s)) into fileInfo
                    select fileInfo
                );
            }
            
            HelmChart? chart = null;
            if (deploymentConfiguration.Renderer.HelmChart?.Name != null && deploymentConfiguration.Renderer.HelmChart.Repository.Url != null )
            {
                NetworkCredential? credential = null;
                
                if (deploymentConfiguration.Renderer.HelmChart.Repository.Username != null &&
                    deploymentConfiguration.Renderer.HelmChart.Repository.Password != null)
                {
                    credential = new NetworkCredential(
                        deploymentConfiguration.Renderer.HelmChart.Repository.Username,
                        deploymentConfiguration.Renderer.HelmChart.Repository.Password
                        );
                }

                var repository = credential switch
                {
                    null => new HelmChartRepository(deploymentConfiguration.Renderer.HelmChart.Repository.Url),
                    _ => new UsernamePasswordAuthHelmChartRepository(
                        deploymentConfiguration.Renderer.HelmChart.Repository.Url,
                        credential
                    )
                };
                
                chart = new HelmChart(
                    deploymentConfiguration.Renderer.HelmChart.Name,
                    repository
                );
            }

            var deploymentRendererContext = new HelmRendererContext(
                deploymentConfiguration,
                GenerateName(),
                GenerateNamespace(),
                _deploymentConfigurationPathProvider.GetDeploymentRepositoryRoot(),
                // ReSharper disable once ArgumentsStyleOther
                chart: chart,
                // ReSharper disable once ArgumentsStyleOther
                cluster: _renderArguments.Value.Cluster ?? _renderConfiguration.Value.Cluster,
                // ReSharper disable once ArgumentsStyleOther
                environment: _renderArguments.Value.Environment ?? _renderConfiguration.Value.Environment,
                // ReSharper disable once ArgumentsStyleOther
                vertical: _renderArguments.Value.Vertical ?? _renderConfiguration.Value.Vertical,
                // ReSharper disable once ArgumentsStyleOther
                subVertical: _renderArguments.Value.SubVertical ?? _renderConfiguration.Value.SubVertical
            );
            
            helmValueFiles
                .ForEach(x =>
                {
                    if (!x.Exists)
                    {
                        if (_globalArguments.Value.Verbose)
                        {
                            Console.WriteLine($"File '{x.FullName}' is missing ...");
                        }
                    }
                    else
                    {
                        if (x.Name.Equals(deploymentConfiguration.Renderer.Secrets.Filename))
                        {
                            var decodedFile = _secretsHandler.Decode(x);
                            deploymentRendererContext.ValueFiles.Add(decodedFile.FullName);
                        }
                        else
                        {
                            deploymentRendererContext.ValueFiles.Add(x.FullName);
                        }
                    }
                });

            return deploymentRendererContext;
        }

        private string GenerateNamespace()
        {
            if (!string.IsNullOrEmpty(_argoCdEnvironment.Value.Namespace))
            {
                return _argoCdEnvironment.Value.Namespace;
            }

            if (!string.IsNullOrEmpty(_renderArguments.Value.Namespace))
            {
                return _renderArguments.Value.Namespace;
            }

            throw new InvalidOperationException("A namespace needs to be specified to be able to render the environment properly.");
        }
        
        private string GenerateName()
        {
            if (!string.IsNullOrEmpty(_argoCdEnvironment.Value.Name))
            {
                if (_globalArguments.Value.Verbose)
                {
                    Console.WriteLine("Using ReleaseName from ARGOCD env");
                }
                return _argoCdEnvironment.Value.Name;
            }

            if (!string.IsNullOrEmpty(_renderArguments.Value.Name))
            {
                if (_globalArguments.Value.Verbose)
                {
                    Console.WriteLine("Using ReleaseName from arguments");
                }
                
                return _renderArguments.Value.Name;
            }
            
            var name =
                $"{_renderArguments.Value.Cluster ?? _renderConfiguration.Value.Cluster}" +
                $"-{_renderArguments.Value.Environment ?? _renderConfiguration.Value.Environment}" +
                $"-{_renderArguments.Value.Vertical ?? _renderConfiguration.Value.Vertical}" +
                $"-{_renderArguments.Value.SubVertical ?? _renderConfiguration.Value.SubVertical}";

            return name.Trim('-');

        }
        
    }

    public interface IDeploymentRendererContextBuilder
    {
        DeploymentRendererContext GenerateDeploymentRendererContext();
    }
}