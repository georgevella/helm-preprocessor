using System.Collections.Generic;

namespace HelmPreprocessor.Configuration
{
    /// <summary>
    ///     The DeploymentConfiguration is loaded from the config/ folder using the combination of env,vertical,cluster and sub-vertical arguments. 
    /// </summary>
    public class DeploymentConfiguration
    {
        public SecretsConfiguration Secrets { get; set; } = new SecretsConfiguration();
        public ServicesConfiguration Services { get; set; } = new ServicesConfiguration();
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