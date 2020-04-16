using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HelmPreprocessor.Configuration;
using HelmPreprocessor.Services;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Commands.Handlers
{
    public class ListConfigurationsCommandHandler : ICommandHandler
    {
        private readonly IOptions<RenderConfiguration> _renderConfiguration;
        private readonly IDeploymentConfigurationPathProvider _deploymentConfigurationPathProvider;

        public ListConfigurationsCommandHandler(
            IOptions<RenderConfiguration> renderConfiguration,
            IDeploymentConfigurationPathProvider deploymentConfigurationPathProvider
        )
        {
            _renderConfiguration = renderConfiguration;
            _deploymentConfigurationPathProvider = deploymentConfigurationPathProvider;
        }
        public Task Run(CancellationToken cancellationToken)
        {
            var deploymentRepositoryRoot = _deploymentConfigurationPathProvider.GetDeploymentRepositoryRoot();

            var deploymentConfigurationRoots = new List<DirectoryInfo>();

            RecusivelyBuildDeploymentConfigurationRoots(deploymentRepositoryRoot, deploymentConfigurationRoots);
            
            Console.WriteLine("Deployment Configuration Roots: ");
            deploymentConfigurationRoots.ForEach( x=>Console.WriteLine($"{x.FullName}"));
            
            return Task.CompletedTask;
        }

        private void RecusivelyBuildDeploymentConfigurationRoots(DirectoryInfo directory, ICollection<DirectoryInfo> deploymentConfigurationRoots)
        {
            if (deploymentConfigurationRoots == null)
                throw new ArgumentNullException(nameof(deploymentConfigurationRoots));
            
            var isDeploymentConfigurationRoot = directory.GetFiles()
                .Any(f =>
                    Constants.APP_VERSIONS_FILENAME.Equals(f.Name) || Constants.PREPROCESSOR_FILENAME.Equals(f.Name)
                );

            if (isDeploymentConfigurationRoot)
            {
                deploymentConfigurationRoots.Add(directory);
                return;
            }
            
            directory.GetDirectories().ToList().ForEach( d => RecusivelyBuildDeploymentConfigurationRoots(d, deploymentConfigurationRoots));
        }
    }
}