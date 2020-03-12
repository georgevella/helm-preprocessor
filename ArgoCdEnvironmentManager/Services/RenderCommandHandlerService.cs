﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HelmPreprocessor.Commands;
using HelmPreprocessor.Configuration;
using HelmPreprocessor.Services.DeploymentRenderers;
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
        private readonly IOptions<GlobalArguments> _globalArguments;
        private readonly ISecretsHandler _secretsHandler;
        private readonly IDeploymentRendererFactory _deploymentRendererFactory;

        public RenderCommandHandlerService(
            IDeploymentConfigurationPathProvider deploymentConfigurationPathProvider,
            IDeploymentConfigurationProvider deploymentConfigurationProvider,
            IOptions<ArgoCdEnvironment> argoCdEnvironment,
            IOptions<RenderConfiguration> renderConfiguration,
            IOptions<RenderArguments> renderArguments,
            IOptions<GlobalArguments> globalArguments,
            ISecretsHandler secretsHandler,
            IDeploymentRendererFactory deploymentRendererFactory
        )
        {
            _deploymentConfigurationPathProvider = deploymentConfigurationPathProvider;
            _deploymentConfigurationProvider = deploymentConfigurationProvider;
            _argoCdEnvironment = argoCdEnvironment;
            _renderConfiguration = renderConfiguration;
            _renderArguments = renderArguments;
            _globalArguments = globalArguments;
            _secretsHandler = secretsHandler;
            _deploymentRendererFactory = deploymentRendererFactory;
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

            var deploymentRenderer = _deploymentRendererFactory.GetDeploymentRenderer(_renderArguments.Value.Renderer);

            var deploymentContext = new HelmRendererContext()
            {
                Name = GenerateName(),
                Namespace = GenerateNamespace(),
                WorkingDirectory = _deploymentConfigurationPathProvider.GetDeploymentRepository().FullName
            };
            
            deploymentRenderer.Initialize(deploymentContext);

            GenerateHelmTemplate(deploymentRenderer, helmValueFiles, deploymentConfiguration);

            return Task.CompletedTask;
        }

        private void GenerateHelmTemplate(
            IDeploymentRenderer deploymentRenderer, 
            List<FileInfo> helmValueFiles, 
            DeploymentConfiguration deploymentConfiguration
            )
        {
            var helmContext = new HelmRendererContext()
            {
                Name = GenerateName(),
                Namespace = GenerateNamespace(),
                WorkingDirectory = _deploymentConfigurationPathProvider.GetDeploymentRepository().FullName,
                
                Cluster = _renderArguments.Value.Cluster ?? _renderConfiguration.Value.Cluster,
                Environment = _renderArguments.Value.Environment ?? _renderConfiguration.Value.Environment,
                Vertical = _renderArguments.Value.Vertical ?? _renderConfiguration.Value.Vertical,
                SubVertical = _renderArguments.Value.SubVertical ?? _renderConfiguration.Value.SubVertical
            };
            
            helmValueFiles
                .ForEach(x =>
                {
                    if (!x.Exists) return;

                    if (x.Name.Equals(deploymentConfiguration.Secrets.Filename))
                    {
                        var decodedFile = _secretsHandler.Decode(x);
                        helmContext.ValueFiles.Add(decodedFile.FullName);
                    }
                    else
                    {
                        helmContext.ValueFiles.Add(x.FullName);
                    }
                });
            
            deploymentRenderer.Render(helmContext);
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

            return null;
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
}