using System;
using System.IO;
using HelmPreprocessor.Configuration;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Services
{
    public class DeploymentConfigurationPathProvider : IDeploymentConfigurationPathProvider
    {
        private readonly IOptions<RenderConfiguration> _renderConfiguration;
        private readonly IOptions<RenderArguments> _renderArguments;

        public DeploymentConfigurationPathProvider(
            IOptions<RenderConfiguration> renderConfiguration,
            IOptions<RenderArguments> renderArguments
        )
        {
            _renderConfiguration = renderConfiguration;
            _renderArguments = renderArguments;
        }
        
        public bool TryGetConfigurationRoot(out DirectoryInfo configurationRootDirectory)
        {
            var renderConfiguration = _renderConfiguration.Value;
            var renderArguments = _renderArguments.Value;
            
            var configurationRoot = renderConfiguration.Configuration;

            if (string.IsNullOrWhiteSpace(configurationRoot))
            {
                var configurationRootValuesAvailable =
                    !string.IsNullOrWhiteSpace(renderArguments.Cluster ?? renderConfiguration.Cluster) &&
                    !string.IsNullOrWhiteSpace(renderArguments.Environment ?? renderConfiguration.Environment) &&
                    !string.IsNullOrWhiteSpace(renderArguments.Vertical ?? renderConfiguration.Vertical) &&
                    !string.IsNullOrWhiteSpace(renderArguments.SubVertical ?? renderConfiguration.SubVertical);
                
                if (configurationRootValuesAvailable)
                {
                    configurationRoot = Path.Combine(
                        renderConfiguration.Repository ?? Environment.CurrentDirectory,
                        "config",
                        (renderArguments.Vertical ?? renderConfiguration.Vertical),
                        $"{renderArguments.Cluster ?? renderConfiguration.Cluster}-{renderArguments.Environment ?? renderConfiguration.Environment}",
                        renderArguments.SubVertical ?? renderConfiguration.SubVertical
                    );
                }
            }

            if (!string.IsNullOrWhiteSpace(configurationRoot))
            {
                configurationRootDirectory = new DirectoryInfo(configurationRoot);
                return true;
            }

            configurationRootDirectory = null;
            return false;
        }
    }
}