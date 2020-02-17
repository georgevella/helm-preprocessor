using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HelmPreprocessor.Commands;
using HelmPreprocessor.Configuration;
using HelmPreprocessor.Extensions;
using HelmPreprocessor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HelmPreprocessor
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            static BaseCommand<RenderCommandHandlerService> RenderEnvironment()
            {
                return new BaseCommand<RenderCommandHandlerService>(
                    "render",
                    "Renders the helm chart and produces the K8S resources that will be deployed to the cluster."
                )
                {
                    new Option(new[] {"-e", "--environment"}, "Name of the environment.")
                    {
                        Argument = new Argument<string>()
                    },
                    new Option(new[] {"-v", "--vertical"}, "Name of the vertical.")
                    {
                        Argument = new Argument<string>()
                    },
                    new Option(new[] {"-c", "--cluster"}, "Name of the cluster.")
                    {
                        Argument = new Argument<string>()
                    },
                    new Option(new[] {"-s", "--subvertical"}, "Name of the subvertical (if used).")
                    {
                        Argument = new Argument<string>()
                    },
                };
            }

            static Command ListEnvironments() => new BaseCommand<ListConfigurationsCommandHandlerService>(
                "list-configurations",
                "List all configurations present in the repository."
            )
            {
                Handler = CommandHandler.Create<IHost>(host => Console.WriteLine("list-environments command"))
            };

            var commandLineBuilder = new CommandLineBuilder(RenderEnvironment())
                .AddCommand(ListEnvironments())
                .UseDefaults()
                .UseHost(
                    extraCliArguments => Host
                        .CreateDefaultBuilder()
                        .ConfigureLogging(builder =>
                            builder.AddFilter("Microsoft.Hosting", LogLevel.Error)
                        ),
                    hostBuilder =>
                    {
                        hostBuilder
                            .ConfigureAppConfiguration((hostContext, builder) =>
                            {
                                builder.AddEnvironmentVariables(prefix: "HELM_");
                            })
                            .ConfigureServices((hostContext, services) =>
                            {
                                //services.AddHostedService<Worker>();
                                services.AddScoped<RenderCommandHandlerService>();
                                services
                                    .AddScoped<IDeploymentConfigurationPathProvider, DeploymentConfigurationPathProvider
                                    >();
                                services.AddScoped<IDeploymentConfigurationProvider, DeploymentConfigurationProvider>();

                                services.Configure<RenderConfiguration>(hostContext.Configuration);
                                services.AddOptions<RenderArguments>().BindCommandLine();

                            });
                    }
                );

            var cli = commandLineBuilder.Build();
            var parseResults = cli.Parse(args);
            return await parseResults.InvokeAsync();
            
        }

        // private static IHostBuilder CreateHostBuilder(string[] args) =>
        //     Host.CreateDefaultBuilder(args)
        //         .ConfigureHostConfiguration(builder =>
        //         {
        //             builder.AddEnvironmentVariables(prefix: "HELM_");
        //             builder.AddCommandLine(args);
        //         })
                
    }
}
