namespace HelmPreprocessor.Configuration
{
    public class RenderConfiguration : RenderArguments
    {
        public string Repository { get; set; }

        public string Configuration { get; set; }
    }
    
    public class RenderArguments
    {
        public string Environment { get; set; }
        
        public string Vertical { get; set; }
        
        public string SubVertical { get; set; }
        
        public string Cluster { get; set; }
    }
}