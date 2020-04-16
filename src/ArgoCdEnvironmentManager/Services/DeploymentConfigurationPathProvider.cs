using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using HelmPreprocessor.Commands.Arguments;
using HelmPreprocessor.Configuration;
using HelmPreprocessor.Extensions;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Services
{
    public interface IDeploymentConfigurationPathProvider
    {
        bool TryGetDeploymentConfigurationRoot(out ConfigurationRoot configurationRootDirectory);
        DirectoryInfo GetDeploymentRepositoryRoot();
    }

    public class ConfigurationRoot
    {
        public static implicit operator DirectoryInfo(ConfigurationRoot configurationRoot) => 
            !configurationRoot.IsSet ? throw new InvalidCastException() : configurationRoot.DirectoryInfo!;
        
        public static implicit operator ConfigurationRoot(DirectoryInfo directoryInfo) => 
            new ConfigurationRoot(directoryInfo);

        private ConfigurationRoot(DirectoryInfo directoryInfo) => 
            (DirectoryInfo, IsSet) = (directoryInfo, true);

        public ConfigurationRoot() => 
            (DirectoryInfo, IsSet) = (null, false);
        

        private DirectoryInfo? DirectoryInfo { get; }

        private bool IsSet { get; }
        public string FullName => IsSet ? DirectoryInfo!.FullName : throw new InvalidOperationException();
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

        public DirectoryInfo GetDeploymentRepositoryRoot()
        {
            return new DirectoryInfo(_renderConfiguration.Value.Repository ?? Environment.CurrentDirectory);
        }
        
        public bool TryGetDeploymentConfigurationRoot(out ConfigurationRoot configurationRootDirectory)
        {
            var renderConfiguration = _renderConfiguration.Value;
            var renderArguments = _renderArguments.Value;


            string? GetCluster() => renderArguments.Cluster ?? renderConfiguration.Cluster;
            string? GetEnvironment() => renderArguments.Environment ?? renderConfiguration.Environment;
            string? GetVertical() => renderArguments.Vertical ?? renderConfiguration.Vertical;
            string? GetSubVertical() => renderArguments.SubVertical ?? renderConfiguration.SubVertical;


            var configurationRoot = renderConfiguration.Configuration;

            if (string.IsNullOrWhiteSpace(configurationRoot))
            {
                var configurationRootValuesAvailable =
                    !string.IsNullOrWhiteSpace(GetCluster()) &&
                    !string.IsNullOrWhiteSpace(GetEnvironment()) &&
                    !string.IsNullOrWhiteSpace(GetVertical()) &&
                    !string.IsNullOrWhiteSpace(GetSubVertical());

                var pathParts = new List<string>
                {
                    GetDeploymentRepositoryRoot().FullName, 
                    "config"
                };

                GetVertical().IsNotNullOrWhitespace(s => pathParts.Add(s));
                
                if (!string.IsNullOrWhiteSpace(GetEnvironment()) && !string.IsNullOrWhiteSpace(GetCluster()))
                {
                    pathParts.Add($"{GetCluster()}-{GetEnvironment()}");
                }
                
                GetSubVertical().IsNotNullOrWhitespace(s => pathParts.Add(s));

                configurationRoot = Path.Combine(pathParts.ToArray());
            }

            if (!string.IsNullOrWhiteSpace(configurationRoot))
            {
                configurationRootDirectory = new DirectoryInfo(configurationRoot);
                return true;
            }
            
            configurationRootDirectory = new ConfigurationRoot();
            return false;
        }
    }
}