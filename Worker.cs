using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HelmPreprocessor.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor
{
    public class Worker
    {
        private readonly IConfiguration _configuration;
        private readonly IOptions<HostConfiguration> _hostConfiguration;
        private readonly IOptions<ServicesConfiguration> _servicesConfiguration;

        public Worker(
            IConfiguration configuration,
            IOptions<HostConfiguration> hostConfiguration,
            IOptions<ServicesConfiguration> servicesConfiguration
        )
        {
            _configuration = configuration;
            _hostConfiguration = hostConfiguration;
            _servicesConfiguration = servicesConfiguration;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var hostConfiguration = _hostConfiguration.Value;
            
            var configurationRoot = Path.Combine(
                hostConfiguration.Repository ?? Environment.CurrentDirectory,
                "config",
                $"{hostConfiguration.Cluster}-{hostConfiguration.Environment}",
                hostConfiguration.SubVertical
            );


            var helmValueFiles = new List<FileInfo>();
            
            helmValueFiles.Add(new FileInfo(Path.Combine(configurationRoot, "values.yaml")));
            helmValueFiles.Add(new FileInfo(Path.Combine(configurationRoot, "app-versions.yaml")));
            helmValueFiles.Add(new FileInfo(Path.Combine(configurationRoot, "secrets.yaml")));

            foreach (var serviceMap in _servicesConfiguration.Value)
            {
                helmValueFiles.AddRange(
                    from s in new[] {"values.yaml", "secrets.yaml", "infra.yaml"}
                    select new FileInfo(Path.Combine(configurationRoot, serviceMap.Key, s)) into fileInfo
                    select fileInfo
                );
            }
            
            helmValueFiles.ForEach(x =>
            {
                
            });
            
            var processStartInfo = new ProcessStartInfo("helm");
            processStartInfo.ArgumentList.Add("template");
            processStartInfo.ArgumentList.Add(".");
            
            helmValueFiles
                .ForEach( x =>
                {
                    if (!x.Exists) return;
                    
                    if (x.Name.Equals("secrets.yaml"))
                    {
                        var psi = new ProcessStartInfo("sops", $"-d -i {x.FullName}");
                        Process.Start(psi)?.WaitForExit();
                    }
                    
                    processStartInfo.ArgumentList.Add("-f");
                    processStartInfo.ArgumentList.Add(x.FullName);
                });

            Process.Start(processStartInfo)?.WaitForExit();
            
            return Task.CompletedTask;
        }
    }
}