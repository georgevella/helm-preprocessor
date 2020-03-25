using System;
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
        private readonly IDeploymentConfigurationProvider _deploymentConfigurationProvider;

        public ListConfigurationsCommandHandler(
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