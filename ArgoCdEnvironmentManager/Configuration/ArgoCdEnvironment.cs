namespace HelmPreprocessor.Configuration
{
    /// <summary>
    ///     Used to capture environment values supplied by ArgoCD
    /// </summary>
    public class ArgoCdEnvironment
    {
        public string? Name { get; set; }
        public string? Namespace { get; set; }
        public string? Revision { get; set; }
        public string? SourcePath { get; set; }
        public string? SourceRepoUrl { get; set; }
        public string? SourceTargetRevision { get; set; }
    }
}