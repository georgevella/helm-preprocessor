using System;
using System.Collections.Generic;
using System.IO;
using HelmPreprocessor.Configuration;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Services
{
    public interface IDeploymentConfigurationPathProvider
    {
        bool TryGetConfigurationRoot(out DirectoryInfo configurationRootDirectory);
        DirectoryInfo GetDeploymentRepository();
    }
    
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

        public DirectoryInfo GetDeploymentRepository()
        {
            return new DirectoryInfo(_renderConfiguration.Value.Repository ?? Environment.CurrentDirectory);
        }
        
        public bool TryGetConfigurationRoot(out DirectoryInfo configurationRootDirectory)
        {
            var renderConfiguration = _renderConfiguration.Value;
            var renderArguments = _renderArguments.Value;


            string GetCluster() => renderArguments.Cluster ?? renderConfiguration.Cluster;
            string GetEnvironment() => renderArguments.Environment ?? renderConfiguration.Environment;
            string GetVertical() => renderArguments.Vertical ?? renderConfiguration.Vertical;
            string GetSubVertical() => renderArguments.SubVertical ?? renderConfiguration.SubVertical;


            var configurationRoot = renderConfiguration.Configuration;

            if (string.IsNullOrWhiteSpace(configurationRoot))
            {
                var configurationRootValuesAvailable =
                    !string.IsNullOrWhiteSpace(renderArguments.Cluster ?? renderConfiguration.Cluster) &&
                    !string.IsNullOrWhiteSpace(renderArguments.Environment ?? renderConfiguration.Environment) &&
                    !string.IsNullOrWhiteSpace(renderArguments.Vertical ?? renderConfiguration.Vertical) &&
                    !string.IsNullOrWhiteSpace(renderArguments.SubVertical ?? renderConfiguration.SubVertical);

                var pathParts = new List<string>();
                pathParts.Add(GetDeploymentRepository().FullName);
                pathParts.Add("config");

                if (!string.IsNullOrWhiteSpace(GetVertical()))
                {
                    pathParts.Add(GetVertical());
                }

                if (!string.IsNullOrWhiteSpace(GetEnvironment()) && !string.IsNullOrWhiteSpace(GetCluster()))
                {
                    pathParts.Add($"{GetCluster()}-{GetEnvironment()}");
                }

                if (!string.IsNullOrWhiteSpace(GetSubVertical()))
                {
                    pathParts.Add(GetSubVertical());
                }

                configurationRoot = Path.Combine(pathParts.ToArray());
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