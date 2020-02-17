using System;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HelmPreprocessor.Commands;
using HelmPreprocessor.Configuration;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Services
{
    public class ListConfigurationsCommandHandlerService : ICommandHandlerService
    {
        private readonly IOptions<RenderConfiguration> _renderConfiguration;
        private readonly IDeploymentConfigurationPathProvider _deploymentConfigurationPathProvider;
        private readonly IDeploymentConfigurationProvider _deploymentConfigurationProvider;

        public ListConfigurationsCommandHandlerService(
            IOptions<RenderConfiguration> renderConfiguration,
            IDeploymentConfigurationPathProvider deploymentConfigurationPathProvider,
            IDeploymentConfigurationProvider deploymentConfigurationProvider
            )
        {
            _renderConfiguration = renderConfiguration;
            _deploymentConfigurationPathProvider = deploymentConfigurationPathProvider;
            _deploymentConfigurationProvider = deploymentConfigurationProvider;
        }
        public Task Run(CancellationToken cancellationToken)
        {
            var renderConfiguration = _renderConfiguration.Value;
            Console.WriteLine($"Repository Path: {renderConfiguration.Repository}");
            
            return Task.CompletedTask;
        }
    }
}