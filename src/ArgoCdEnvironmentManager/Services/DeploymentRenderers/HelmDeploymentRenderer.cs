using System.Diagnostics;
using System.IO;
using System.Text;
using HelmPreprocessor.Commands.Arguments;
using HelmPreprocessor.Configuration;
using HelmPreprocessor.Extensions;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Services.DeploymentRenderers
{
    abstract class BaseHelmDeploymentRenderer : IDeploymentRenderer
    {
        private readonly IOptions<ArgoCdEnvironment> _argoCdEnvironment;
        private readonly IOptions<RenderConfiguration> _renderConfiguration;
        private readonly IOptions<RenderArguments> _renderArguments;
        private readonly IOptions<GeneralArguments> _globalArguments;

        protected BaseHelmDeploymentRenderer(
            IOptions<ArgoCdEnvironment> argoCdEnvironment,
            IOptions<RenderConfiguration> renderConfiguration,
            IOptions<RenderArguments> renderArguments,
            IOptions<GeneralArguments> globalArguments
        )
        {
            _argoCdEnvironment = argoCdEnvironment;
            _renderConfiguration = renderConfiguration;
            _renderArguments = renderArguments;
            _globalArguments = globalArguments;
        }
        
        protected void FetchHelmDependencies(DeploymentRendererContext context)
        {
            var requirementsLockFile = context.WorkingDirectory.GetFilePath("requirements.lock");
             
            if (requirementsLockFile.Exists)
            {
                requirementsLockFile.Delete();
            }

            var processStartInfo = new ProcessStartInfo("helm")
            {
                WorkingDirectory = context.WorkingDirectory.FullName,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            
            processStartInfo.ArgumentList.Add("dependency");
            processStartInfo.ArgumentList.Add("build");

            var process = new Process()
            {
                StartInfo = processStartInfo
            };
            process.OutputDataReceived += (sender, args) => { /* do nothing */  };
            process.ErrorDataReceived += (sender, args) => { /* do nothing */ };
            process.Start();
            process.WaitForExit();
        }
        
        public abstract void Initialize(DeploymentRendererContext context);

        public abstract void Fetch(DeploymentRendererContext context);
        
        public abstract void Render(DeploymentRendererContext context);
    }
}