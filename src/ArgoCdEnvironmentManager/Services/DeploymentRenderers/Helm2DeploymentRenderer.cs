using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HelmPreprocessor.Commands.Arguments;
using HelmPreprocessor.Configuration;
using HelmPreprocessor.Extensions;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Services.DeploymentRenderers
{
    class Helm2DeploymentRenderer : BaseHelmDeploymentRenderer
    {
        private readonly IOptions<GeneralArguments> _globalArguments;

        public Helm2DeploymentRenderer(
            IOptions<ArgoCdEnvironment> argoCdEnvironment,
            IOptions<RenderConfiguration> renderConfiguration,
            IOptions<RenderArguments> renderArguments,
            IOptions<GeneralArguments> globalArguments
        ) : base(argoCdEnvironment, renderConfiguration, renderArguments, globalArguments)
        {
            _globalArguments = globalArguments;
        }
        
        
        public override void Initialize(DeploymentRendererContext context)
        {
            FetchHelmDependencies(context);
        }

        public override void Fetch(DeploymentRendererContext context)
        {
            var helmContext = (HelmRendererContext) context;
            var chart = helmContext.Chart!;
            
            var processStartInfo = new ProcessStartInfo("helm")
            {
                WorkingDirectory = context.WorkingDirectory.FullName
            };
            processStartInfo.ArgumentList.Add("fetch");
            processStartInfo.ArgumentList.Add(chart.Name);

            chart.Version.IsNotNullOrEmpty(s =>
            {
                processStartInfo.ArgumentList.Add("--version");
                processStartInfo.ArgumentList.Add(s);
            });
            
            processStartInfo.ArgumentList.Add("--repo");
            processStartInfo.ArgumentList.Add(chart.Repository.Url);

            chart.Repository.Is<UsernamePasswordAuthHelmChartRepository>()
                .Do(authHelmChartRepository =>
                    {
                        processStartInfo.ArgumentList.Add("--username");
                        processStartInfo.ArgumentList.Add(authHelmChartRepository.Credentials.UserName);
                        processStartInfo.ArgumentList.Add("--password");
                        processStartInfo.ArgumentList.Add(authHelmChartRepository.Credentials.Password);
                    }
                );
            var chartOutputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("D"));
            processStartInfo.ArgumentList.Add("--destination");
            processStartInfo.ArgumentList.Add(chartOutputPath);
            processStartInfo.ArgumentList.Add("--untar");
            
            // helm always extracts the chart to a subfolder, so we get the first subdirectory 
            var chartOutputDirectory = new DirectoryInfo(chartOutputPath);
            var chartDirectory = chartOutputDirectory.GetDirectories().First();
            
            chartDirectory.CopyRecursive(context.WorkingDirectory);
        }

        public override void Render(DeploymentRendererContext context)
        {
            var helmContext = (HelmRendererContext) context;

            var processStartInfo = new ProcessStartInfo("helm")
            {
                WorkingDirectory = context.WorkingDirectory.FullName
            };
            processStartInfo.ArgumentList.Add("template");
            processStartInfo.ArgumentList.Add(".");
            
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
}