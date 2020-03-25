using System.Collections.Generic;

namespace HelmPreprocessor.Services.DeploymentRenderers
{
    public interface IDeploymentRenderer
    {
        void Initialize(DeploymentRendererContext context);
        
        void Render(DeploymentRendererContext context);
    }

    public class DeploymentRendererContext
    {
        public string? Name { get; set; }
        
        public string? Namespace { get; set; }
        
        public string? WorkingDirectory { get; set; }
        
        public string? Cluster { get; set; }
        public string? Environment { get; set; }
        public string? Vertical { get; set; }
        public string? SubVertical { get; set; }
    }

    public class HelmRendererContext : DeploymentRendererContext
    {
        public List<string> ValueFiles { get; set; } = new List<string>();
    }
}