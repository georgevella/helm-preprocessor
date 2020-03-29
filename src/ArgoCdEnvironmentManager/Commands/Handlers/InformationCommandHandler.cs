using System;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.Threading;
using System.Threading.Tasks;
using HelmPreprocessor.Services;

namespace HelmPreprocessor.Commands.Handlers
{
    public class InformationCommandHandler : ICommandHandler
    {
        private readonly IDeploymentRendererContextBuilder _deploymentRendererContextBuilder;
        private readonly IDeploymentConfigurationProvider _deploymentConfigurationProvider;

        public InformationCommandHandler(
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

            Console.WriteLine("Information");
            Console.WriteLine("~~~~~~~~~~~");
            
            
            Console.WriteLine("Name: {0}", deploymentRendererContext.Name);
            Console.WriteLine("Namespace: {0}", deploymentRendererContext.Namespace);
            Console.WriteLine("Working Directory: {0}", deploymentRendererContext.WorkingDirectory);
            Console.WriteLine("");
            Console.WriteLine("Cluster: {0}", deploymentRendererContext.Cluster ?? "<unset>");
            Console.WriteLine("Environment: {0}", deploymentRendererContext.Environment ?? "<unset>");
            Console.WriteLine("Vertical: {0}", deploymentRendererContext.Vertical ?? "<unset>");
            Console.WriteLine("Sub-Vertical: {0}", deploymentRendererContext.SubVertical ?? "<unset>");
            Console.WriteLine("");
            Console.WriteLine("Services:");
            
            if (_deploymentConfigurationProvider.GetDeploymentConfiguration(out var deploymentConfiguration))
            {
                foreach (var deploymentConfigurationService in deploymentConfiguration.Services)
                {
                    Console.WriteLine($"*  {deploymentConfigurationService.Key} (runtime: {deploymentConfigurationService.Value.Runtime})");
                    Console.WriteLine($"   - image: {deploymentConfigurationService.Value.Image.Repository}:{deploymentConfigurationService.Value.Image.Tag}");
                }
            }

            return Task.CompletedTask;
        }
    }
}
