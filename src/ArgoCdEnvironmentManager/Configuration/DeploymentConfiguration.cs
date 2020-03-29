using System.Collections.Generic;

namespace HelmPreprocessor.Configuration
{
    /// <summary>
    ///     The DeploymentConfiguration is loaded from the config/ folder using the combination of env,vertical,cluster and sub-vertical arguments. 
    /// </summary>
    public class DeploymentConfiguration
    {
        public RendererSettings Renderer { get; } = new RendererSettings();
        public ServicesConfiguration Services { get; } = new ServicesConfiguration();
    }

    public class RendererSettings
    {
        public RendererType Type { get; set; } = RendererType.Helm;
        
        public SecretsConfiguration Secrets { get; } = new SecretsConfiguration();
        
        public HelmChartSettings HelmChart { get; } = new HelmChartSettings();
    }

    public enum RendererType
    {
        Helm
    }

    public class HelmChartSettings
    {
        public string? Name { get; set; }

        public ChartRepositorySettings Repository { get; } = new ChartRepositorySettings();
    }

    public class ChartRepositorySettings
    {
        public string? Url { get; set; }
        
        public string? Username { get; set; }
        
        public string? Password { get; set; }
    }

    /// <summary>
    ///     Describes how secrets are managed within a deployment configuration.
    /// </summary>
    public class SecretsConfiguration
    {
        /// <summary>
        ///     Encrypted file handler.
        /// </summary>
        public SecretsHandlerType Handler { get; set; } = SecretsHandlerType.Sops;

        /// <summary>
        ///     Filename that signals the renderer that the file is encrypted.
        /// </summary>
        public string Filename { get; set; } = "secrets.yaml";
    }

    public enum SecretsHandlerType
    {
        Sops
    }

    public class ServicesConfiguration : Dictionary<string, ServiceConfiguration>
    {
        
    }

    public class ServiceConfiguration
    {
        public string? Runtime { get; set; }
    }
}