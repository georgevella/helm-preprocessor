using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HelmPreprocessor.Commands;
using HelmPreprocessor.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Services
{
    public class RenderCommandHandlerService : ICommandHandlerService
    {
        private readonly IDeploymentConfigurationPathProvider _deploymentConfigurationPathProvider;
        private readonly IDeploymentConfigurationProvider _deploymentConfigurationProvider;
        private readonly IOptions<ArgoCdEnvironment> _argoCdEnvironment;
        private readonly IOptions<RenderConfiguration> _renderConfiguration;
        private readonly IOptions<RenderArguments> _renderArguments;
        private readonly ISecretsHandler _secretsHandler;

        public RenderCommandHandlerService(
            IDeploymentConfigurationPathProvider deploymentConfigurationPathProvider,
            IDeploymentConfigurationProvider deploymentConfigurationProvider,
            IOptions<ArgoCdEnvironment> argoCdEnvironment,
            IOptions<RenderConfiguration> renderConfiguration,
            IOptions<RenderArguments> renderArguments,
            ISecretsHandler secretsHandler
        )
        {
            _deploymentConfigurationPathProvider = deploymentConfigurationPathProvider;
            _deploymentConfigurationProvider = deploymentConfigurationProvider;
            _argoCdEnvironment = argoCdEnvironment;
            _renderConfiguration = renderConfiguration;
            _renderArguments = renderArguments;
            _secretsHandler = secretsHandler;
        }
        
        public Task Run(CancellationToken cancellationToken)
        {
            if (!_deploymentConfigurationPathProvider.TryGetConfigurationRoot(out var configurationRoot))
                return Task.CompletedTask;
            
            if (!_deploymentConfigurationProvider.GetDeploymentConfiguration(out var deploymentConfiguration))
                return Task.CompletedTask;

            var servicesConfiguration = deploymentConfiguration.Services;

            // start building list of helm value files
            var helmValueFiles = new List<FileInfo>
            {
                new FileInfo(Path.Combine(configurationRoot.FullName, "values.yaml")),
                new FileInfo(Path.Combine(configurationRoot.FullName, "app-versions.yaml")),
                new FileInfo(Path.Combine(configurationRoot.FullName, "secrets.yaml"))
            };
            
            foreach (var serviceMap in servicesConfiguration)
            {
                helmValueFiles.AddRange(
                    from s in new[] {"values.yaml", "secrets.yaml", "infra.yaml"}
                    select new FileInfo(Path.Combine(configurationRoot.FullName, serviceMap.Key, s)) into fileInfo
                    select fileInfo
                );
            }

            var processStartInfo = new ProcessStartInfo("helm");
            processStartInfo.ArgumentList.Add("template");
            processStartInfo.ArgumentList.Add(".");
            
            helmValueFiles
                .ForEach( async x =>
                {
                    if (!x.Exists) return;
                    
                    if (x.Name.Equals(deploymentConfiguration.Secrets.Filename))
                    {
                        // var temporaryFile = Path.Combine(x.DirectoryName, $"{x.Name}-dec.yaml");
                        // x.CopyTo(temporaryFile, true);
                        // var psi = new ProcessStartInfo("sops", $"-d -i {temporaryFile}");
                        // Process.Start(psi)?.WaitForExit();

                        var decodedFile = await _secretsHandler.Decode(x);
                        
                        processStartInfo.ArgumentList.Add("-f");
                        processStartInfo.ArgumentList.Add(decodedFile.FullName);
                    }
                    else
                    {
                        processStartInfo.ArgumentList.Add("-f");
                        processStartInfo.ArgumentList.Add(x.FullName);   
                    }
                });
            
            
            processStartInfo.ArgumentList.Add("--name");
            processStartInfo.ArgumentList.Add(GenerateReleaseName());

            Process.Start(processStartInfo)?.WaitForExit();
            
            return Task.CompletedTask;
        }

        private string GenerateReleaseName()
        {
            if (string.IsNullOrEmpty(_argoCdEnvironment.Value.Name))
            {
                return string.Format(
                    "{0}-{1}-{2}-{3}",
                    _renderArguments.Value.Cluster ?? _renderConfiguration.Value.Cluster,
                    _renderArguments.Value.Environment ?? _renderConfiguration.Value.Environment,
                    _renderArguments.Value.Vertical ?? _renderConfiguration.Value.Vertical,
                    _renderArguments.Value.SubVertical ?? _renderConfiguration.Value.SubVertical
                );
            }

            return _argoCdEnvironment.Value.Name;
        }
    }
}