using System.Collections.Generic;

namespace HelmPreprocessor.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class DeploymentConfiguration
    {
        public SecretsConfiguration Secrets { get; set; } = new SecretsConfiguration();
        public ServicesConfiguration Services { get; set; } = new ServicesConfiguration();
    } 

    public class SecretsConfiguration
    {
        public SecretsHandlerType Handler { get; set; } = SecretsHandlerType.Sops;

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
        public string Runtime { get; set; }
    }
}