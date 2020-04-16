using System;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.Threading;
using System.Threading.Tasks;
using HelmPreprocessor.Services;
using YamlDotNet.Serialization;

namespace HelmPreprocessor.Commands.Handlers
{
    public class DiagnosticsCommandHandler : ICommandHandler
    {
        private readonly IDeploymentRendererContextBuilder _deploymentRendererContextBuilder;
        private readonly IDeploymentConfigurationProvider _deploymentConfigurationProvider;

        public DiagnosticsCommandHandler(
            IDeploymentRendererContextBuilder deploymentRendererContextBuilder,
            IDeploymentConfigurationProvider deploymentConfigurationProvider
        )
        {
            _deploymentRendererContextBuilder = deploymentRendererContextBuilder;
            _deploymentConfigurationProvider = deploymentConfigurationProvider;
        }
        public Task Run(CancellationToken cancellationToken)
        {
            var deploymentRendererContext = _deploymentRendererContextBuilder.GenerateDeploymentRendererContext();

            if (_deploymentConfigurationProvider.GetDeploymentConfiguration(out var deploymentConfiguration))
            {
                var yamlSerializer = new Serializer();
                yamlSerializer.Serialize(Console.Out, deploymentConfiguration);
                Console.WriteLine("---");
            
                yamlSerializer.Serialize(Console.Out, deploymentRendererContext);
            }
            
            return Task.CompletedTask;
        }
    }
}
