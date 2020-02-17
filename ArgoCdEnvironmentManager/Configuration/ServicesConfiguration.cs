using System.Collections.Generic;

namespace HelmPreprocessor.Configuration
{
    public class ServicesConfiguration : Dictionary<string, ServiceConfiguration>
    {
        
    }

    public class DeploymentConfiguration
    {
        public ServicesConfiguration Services { get; set; }
    }

    public class ServiceConfiguration
    {
        public string Runtime { get; set; }
    }
}