using System.IO;
using System.Threading.Tasks;
using HelmPreprocessor.Configuration;
using Microsoft.Extensions.Configuration;

namespace HelmPreprocessor.Services
{
    public interface IDeploymentConfigurationProvider
    {
        bool GetDeploymentConfiguration(out DeploymentConfiguration deploymentConfiguration);
    }
    
    public class DeploymentConfigurationProvider : IDeploymentConfigurationProvider
    {
        private readonly IDeploymentConfigurationPathProvider _deploymentConfigurationPathProvider;

        public DeploymentConfigurationProvider(
            IDeploymentConfigurationPathProvider deploymentConfigurationPathProvider
            )
        {
            _deploymentConfigurationPathProvider = deploymentConfigurationPathProvider;
        }

        public bool GetDeploymentConfiguration(out DeploymentConfiguration deploymentConfiguration)
        {
            if (!_deploymentConfigurationPathProvider.TryGetConfigurationRoot(out var configurationRoot))
            {
                deploymentConfiguration = DeploymentConfiguration.Empty;
                return false;
            }
            
            // load deployment configuration from target location
            var rendererConfigurationBuilder = new ConfigurationBuilder();

            var paths = new[]
            {
                "preprocessor.yaml",
                "values.yaml",
                "app-versions.yaml",
            };
            
            foreach (var path in paths)
            {
                var fi = new FileInfo(Path.Combine(configurationRoot.FullName, path));
                if (fi.Exists)
                {
                    rendererConfigurationBuilder.AddYamlFile(fi.FullName);
                }    
            }
            var renderConfiguration = rendererConfigurationBuilder.Build();
            
            // 
            deploymentConfiguration = new DeploymentConfiguration();
            renderConfiguration.Bind(deploymentConfiguration);

            return true;
        }
    }
}