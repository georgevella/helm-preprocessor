using System;
using System.Diagnostics;
using System.IO;
using HelmPreprocessor.Configuration;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Services.DeploymentRenderers
{
    abstract class BaseHelmDeploymentRenderer : IDeploymentRenderer
    {
        private readonly IOptions<ArgoCdEnvironment> _argoCdEnvironment;
        private readonly IOptions<RenderConfiguration> _renderConfiguration;
        private readonly IOptions<RenderArguments> _renderArguments;
        private readonly IOptions<GlobalArguments> _globalArguments;

        protected BaseHelmDeploymentRenderer(
            IOptions<ArgoCdEnvironment> argoCdEnvironment,
            IOptions<RenderConfiguration> renderConfiguration,
            IOptions<RenderArguments> renderArguments,
            IOptions<GlobalArguments> globalArguments
        )
        {
            _argoCdEnvironment = argoCdEnvironment;
            _renderConfiguration = renderConfiguration;
            _renderArguments = renderArguments;
            _globalArguments = globalArguments;
        }
        
        protected void FetchHelmDependencies(DeploymentRendererContext context)
        {
            var requirementsLockFile = Path.Combine(
                context.WorkingDirectory, 
                "requirements.lock"
            );
            if (File.Exists(requirementsLockFile))
            {
                File.Delete(requirementsLockFile);
            }
            
            var processStartInfo = new ProcessStartInfo("helm");
            processStartInfo.ArgumentList.Add("dependency");
            processStartInfo.ArgumentList.Add("build");
            processStartInfo.WorkingDirectory = context.WorkingDirectory;
            processStartInfo.RedirectStandardError = 
                processStartInfo.RedirectStandardOutput = true;

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
        public abstract void Render(DeploymentRendererContext context);
    }
    
    class Helm2DeploymentRenderer : BaseHelmDeploymentRenderer
    {
        private readonly IOptions<GlobalArguments> _globalArguments;

        public Helm2DeploymentRenderer(
            IOptions<ArgoCdEnvironment> argoCdEnvironment,
            IOptions<RenderConfiguration> renderConfiguration,
            IOptions<RenderArguments> renderArguments,
            IOptions<GlobalArguments> globalArguments
        ) : base(argoCdEnvironment, renderConfiguration, renderArguments, globalArguments)
        {
            _globalArguments = globalArguments;
        }
        
        
        public override void Initialize(DeploymentRendererContext context)
        {
            FetchHelmDependencies(context);
        }

        public override void Render(DeploymentRendererContext context)
        {
            var helmContext = (HelmRendererContext) context;
            
            var processStartInfo = new ProcessStartInfo("helm");
            processStartInfo.ArgumentList.Add("template");
            processStartInfo.ArgumentList.Add(".");
            processStartInfo.WorkingDirectory = context.WorkingDirectory;
            
            helmContext.ValueFiles.ForEach(x =>
            {
                processStartInfo.ArgumentList.Add("-f");
                processStartInfo.ArgumentList.Add( x );
            });
            
            processStartInfo.ArgumentList.Add("--name");
            processStartInfo.ArgumentList.Add(context.Name);
            
            var namespaceName = context.Namespace;
            if (!string.IsNullOrEmpty(namespaceName))
            {
                processStartInfo.ArgumentList.Add("--namespace");
                processStartInfo.ArgumentList.Add(namespaceName);
            }

            if (_globalArguments.Value.Verbose)
            {
                Console.WriteLine($"{processStartInfo.FileName} {string.Join(" ", processStartInfo.ArgumentList)}");
            }

            Process.Start(processStartInfo)?.WaitForExit();
        }
    }
    
    class Helm3DeploymentRenderer : BaseHelmDeploymentRenderer
    {
        private readonly IOptions<GlobalArguments> _globalArguments;

        public Helm3DeploymentRenderer(
            IOptions<ArgoCdEnvironment> argoCdEnvironment,
            IOptions<RenderConfiguration> renderConfiguration,
            IOptions<RenderArguments> renderArguments,
            IOptions<GlobalArguments> globalArguments
        ) : base(argoCdEnvironment, renderConfiguration, renderArguments, globalArguments)
        {
            _globalArguments = globalArguments;
        }
        
        
        public override void Initialize(DeploymentRendererContext context)
        {
            FetchHelmDependencies(context);
        }

        public override void Render(DeploymentRendererContext context)
        {
            var helmContext = (HelmRendererContext) context;
            
            var processStartInfo = new ProcessStartInfo("helm3");
            processStartInfo.ArgumentList.Add("template");
            processStartInfo.ArgumentList.Add(context.Name);
            processStartInfo.ArgumentList.Add(".");
            processStartInfo.WorkingDirectory = context.WorkingDirectory;
            
            helmContext.ValueFiles.ForEach(x =>
            {
                processStartInfo.ArgumentList.Add("-f");
                processStartInfo.ArgumentList.Add( x );
            });

            var namespaceName = context.Namespace;
            if (!string.IsNullOrEmpty(namespaceName))
            {
                processStartInfo.ArgumentList.Add("--namespace");
                processStartInfo.ArgumentList.Add(namespaceName);
            }

            if (_globalArguments.Value.Verbose)
            {
                Console.WriteLine($"{processStartInfo.FileName} {string.Join(" ", processStartInfo.ArgumentList)}");
            }

            Process.Start(processStartInfo)?.WaitForExit();
        }
    }
}