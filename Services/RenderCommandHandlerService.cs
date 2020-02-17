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

        public RenderCommandHandlerService(
            IDeploymentConfigurationPathProvider deploymentConfigurationPathProvider,
            IDeploymentConfigurationProvider deploymentConfigurationProvider
        )
        {
            _deploymentConfigurationPathProvider = deploymentConfigurationPathProvider;
            _deploymentConfigurationProvider = deploymentConfigurationProvider;
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
                .ForEach( x =>
                {
                    if (!x.Exists) return;
                    
                    if (x.Name.Equals("secrets.yaml"))
                    {
                        var temporaryFile = Path.Combine(x.DirectoryName, $"{x.Name}-dec.yaml");
                        x.CopyTo(temporaryFile, true);
                        var psi = new ProcessStartInfo("sops", $"-d -i {temporaryFile}");
                        Process.Start(psi)?.WaitForExit();
                        
                        processStartInfo.ArgumentList.Add("-f");
                        processStartInfo.ArgumentList.Add(temporaryFile);
                    }
                    else
                    {
                        processStartInfo.ArgumentList.Add("-f");
                        processStartInfo.ArgumentList.Add(x.FullName);   
                    }
                });

            Process.Start(processStartInfo)?.WaitForExit();
            
            return Task.CompletedTask;
        }
    }
}