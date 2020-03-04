namespace HelmPreprocessor.Configuration
{
    /// <summary>
    ///     Captures values supplied to the service via Environment Variables.
    /// </summary>
    public class RenderConfiguration : RenderArguments
    {
        /// <summary>
        ///     Root of the git repository that contains the deployment specification.
        /// </summary>
        public string Repository { get; set; }

        /// <summary>
        ///     Specifies the location (relative to the repository root) where the environment configuration is located.
        ///     If not supplied, this value is built from values supplied by <c>RenderArguments</c>.  
        /// </summary>
        public string Configuration { get; set; }
    }
    
    /// <summary>
    ///     Captures the values supplied via environment variables (HELM_*) or command line arguments.
    /// </summary>
    public class RenderArguments
    {
        public string Environment { get; set; }
        
        public string Vertical { get; set; }
        
        public string SubVertical { get; set; }
        
        public string Cluster { get; set; }
        
        public string Namespace { get; set; }
        
        public string Name { get; set; }

        public string Renderer { get; set; } = "helm2";
    }
}