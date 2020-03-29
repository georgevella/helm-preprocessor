using HelmPreprocessor.Commands.Arguments;

namespace HelmPreprocessor.Configuration
{
    /// <summary>
    ///     Captures values supplied to the service via Environment Variables (generally HELM_*)
    /// </summary>
    public class RenderConfiguration : RenderArguments
    {
        /// <summary>
        ///     Root of the git repository that contains the deployment specification.
        /// </summary>
        public string? Repository { get; set; }

        /// <summary>
        ///     Specifies the location (relative to the repository root) where the environment configuration is located.
        ///     If not supplied, this value is built from values supplied by <c>RenderArguments</c>.  
        /// </summary>
        public string? Configuration { get; set; }
    }
}