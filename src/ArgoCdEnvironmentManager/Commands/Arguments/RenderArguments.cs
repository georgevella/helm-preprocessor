namespace HelmPreprocessor.Commands.Arguments
{
    /// <summary>
    ///     Captures the values supplied via environment variables (HELM_*) or command line arguments.
    /// </summary>
    public class RenderArguments
    {
        public string? Environment { get; set; }
        
        public string? Vertical { get; set; }
        
        public string? SubVertical { get; set; }
        
        public string? Cluster { get; set; }
        
        public string? Namespace { get; set; }
        
        public string? Name { get; set; }

        public string Renderer { get; set; } = "helm2";
    }
}