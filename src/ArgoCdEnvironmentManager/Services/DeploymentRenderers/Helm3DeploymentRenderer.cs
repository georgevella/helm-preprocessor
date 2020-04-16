using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using HelmPreprocessor.Commands.Arguments;
using HelmPreprocessor.Configuration;
using HelmPreprocessor.Extensions;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Services.DeploymentRenderers
{
    class Helm3DeploymentRenderer : BaseHelmDeploymentRenderer
    {
        private readonly IOptions<GeneralArguments> _globalArguments;

        public Helm3DeploymentRenderer(
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
            var helmContext = (HelmRendererContext) context;
            if (helmContext.Chart != null)
            {
                Fetch(context);
            }
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

            Process.Start(processStartInfo).WaitForExit();
            
            // helm always extracts the chart to a subfolder, so we get the first subdirectory 
            var chartOutputDirectory = new DirectoryInfo(chartOutputPath);
            var chartDirectory = chartOutputDirectory.GetDirectories().First();
            
            chartDirectory.CopyRecursive(context.WorkingDirectory);
        }

        public override void Render(DeploymentRendererContext context)
        {
            var helmContext = (HelmRendererContext) context;
            
            var processStartInfo = new ProcessStartInfo("helm3");
            processStartInfo.ArgumentList.Add("template");
            processStartInfo.ArgumentList.Add(context.Name);
            processStartInfo.ArgumentList.Add(".");
            processStartInfo.WorkingDirectory = context.WorkingDirectory.FullName;
            
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

            var environmentalVariables = new List<string>();

            if (context.Cluster != null)
            {
                environmentalVariables.Add($"cluster={context.Cluster}");
            }
            
            if (context.Environment != null)
            {
                environmentalVariables.Add($"environment={context.Environment}");
            }
            
            if (context.SubVertical != null)
            {
                environmentalVariables.Add($"subvertical={context.SubVertical}");
            }
            
            if (context.Vertical != null)
            {
                environmentalVariables.Add($"vertical={context.Vertical}");
            }
            
            processStartInfo.ArgumentList.Add("--set-string");
            processStartInfo.ArgumentList.Add(string.Join(",", environmentalVariables));

            if (_globalArguments.Value.Verbose)
            {
                Console.WriteLine($"{processStartInfo.FileName} {string.Join(" ", processStartInfo.ArgumentList)}");
            }

            Process.Start(processStartInfo)?.WaitForExit();
        }
    }
}