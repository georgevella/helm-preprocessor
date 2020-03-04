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
using HelmPreprocessor.Services.DeploymentRenderers;
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
            static BaseCommand<RenderCommandHandlerService> RenderEnvironment() =>
                new BaseCommand<RenderCommandHandlerService>(
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
                    new Option(new[] {"-n", "--namespace"}, "Name of namespace.")
                    {
                        Argument = new Argument<string>()
                    },
                    new Option(new[] {"--name"}, "Name of the release.")
                    {
                        Argument = new Argument<string>()
                    },
                    
                    new Option(new[] {"--renderer"}, "Renderer to use to generate the chart.")
                    {
                        Argument = new Argument<string>()
                        {
                            
                        },
                        Required = false
                    },
                };

            static Command ListEnvironments() => new BaseCommand<ListConfigurationsCommandHandlerService>(
                "list-configurations",
                "List all configurations present in the repository."
            );

            var commandLineBuilder = new CommandLineBuilder()
                .AddCommand(ListEnvironments())
                .AddCommand(RenderEnvironment())
                .AddOption(new Option(new[] {"--verbose"}))
                .UseDefaults()
                .UseHost(
                    extraCliArguments => new HostBuilder()
                        .UseDefaultServiceProvider((context, options) => { })
                        .ConfigureHostConfiguration(builder =>
                        {
                            builder.AddEnvironmentVariables(prefix: "DOTNET_");
                        })
                        .ConfigureLogging((hostingContext, logging) =>
                            {
                                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                                logging.AddConsole();
                                logging.AddDebug();
                                logging.AddEventSourceLogger();
                                logging.AddFilter("Microsoft.Hosting", LogLevel.Error);
                            }
                        ),
                    hostBuilder =>
                    {
                        hostBuilder
                            .ConfigureAppConfiguration((hostContext, builder) =>
                            {
                                var env = hostContext.HostingEnvironment;
                                
                                builder.AddEnvironmentVariables(prefix: "HELM_");
                                builder.AddEnvironmentVariables(prefix: "ARGOCD_APP_");
                                
                                builder
                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                            })
                            .ConfigureServices((hostContext, services) =>
                            {
                                //services.AddHostedService<Worker>();
                                services.AddScoped<RenderCommandHandlerService>();
                                services
                                    .AddScoped<IDeploymentConfigurationPathProvider, DeploymentConfigurationPathProvider
                                    >();
                                services.AddScoped<IDeploymentConfigurationProvider, DeploymentConfigurationProvider>();

                                services.AddScoped<ISecretsHandler, SopsSecretsHandler>();
                                
                                services.Configure<RenderConfiguration>(hostContext.Configuration);
                                
                                services.Configure<ArgoCdEnvironment>(hostContext.Configuration);

                                services.AddScoped<IDeploymentRenderer, Helm2DeploymentRenderer>();
                                services.AddScoped<IDeploymentRenderer, Helm3DeploymentRenderer>();
                                services.AddSingleton<IDeploymentRendererFactory, DeploymentRendererFactory>();
                                
                                services.AddOptions<RenderArguments>().BindCommandLine();
                                services.AddOptions<GlobalArguments>().BindCommandLine();

                            });
                    }
                );

            var cli = commandLineBuilder.Build();
            var parseResults = cli.Parse(args);
            return await parseResults.InvokeAsync();
            
        }

    }
}
