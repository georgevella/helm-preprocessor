using System.IO;

namespace HelmPreprocessor.Services.DeploymentRenderers
{
    public interface IDeploymentRenderer
    {
        void Initialize(DeploymentRendererContext context);

        void Fetch(DeploymentRendererContext context);
        
        void Render(DeploymentRendererContext context);
    }

    public class DeploymentRendererContext
    {
        public string Name { get; }
        
        public string Namespace { get; }
        
        public DirectoryInfo WorkingDirectory { get; }
        
        public string? Cluster { get; }
        public string? Environment { get; }
        public string? Vertical { get;  }
        public string? SubVertical { get; }
        
        public DeploymentRendererContext(string name, string ns, DirectoryInfo workingDirectory, string? cluster,
            string? environment, string? vertical, string? subVertical)
        {
            Name = name;
            Namespace = ns;
            WorkingDirectory = workingDirectory;
            
            Cluster = cluster;
            Environment = environment;
            Vertical = vertical;
            SubVertical = subVertical;
        }
    }
}