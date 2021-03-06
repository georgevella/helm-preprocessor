﻿using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using HelmPreprocessor.Commands;
using HelmPreprocessor.Commands.Arguments;
using HelmPreprocessor.Commands.Handlers;
using HelmPreprocessor.Configuration;
using HelmPreprocessor.Services;
using HelmPreprocessor.Services.DeploymentRenderers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace HelmPreprocessor
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            static BaseCommand<RenderCommandHandler> RenderEnvironment() =>
                new BaseCommand<RenderCommandHandler>(
                    "render",
                    description: "Renders the helm chart and produces the K8S resources that will be deployed to the cluster."
                )
                {
                    new Option(new[] {"-e", "--environment"}, "Name of the environment.")
                    {
                        Argument = new Argument<string>(),
                        Required = false
                    },
                    new Option(new[] {"-v", "--vertical"}, "Name of the vertical.")
                    {
                        Argument = new Argument<string>(),
                        Required = false
                    },
                    new Option(new[] {"-c", "--cluster"}, "Name of the cluster.")
                    {
                        Argument = new Argument<string>(),
                        Required = false
                    },
                    new Option(new[] {"-s", "--subvertical"}, "Name of the sub-vertical (if used).")
                    {
                        Argument = new Argument<string>(),
                        Required = false
                    },
                    new Option(new[] {"-n", "--namespace"}, "Name of namespace.")
                    {
                        Argument = new Argument<string>(),
                        Required = false
                    },
                    new Option(new[] {"--name"}, "Name of the release.")
                    {
                        Argument = new Argument<string>(),
                        Required = false
                    },
                    new Option(new[] {"--renderer"}, "Renderer to use to generate the chart.")
                    {
                        Argument = new Argument<string>(),
                        Required = false
                    },
                };
            
            static BaseCommand<InformationCommandHandler> InformationCommand() =>
                new BaseCommand<InformationCommandHandler>(
                    "info",
                    description: "Displays information about the selected deployment configuration."
                )
                {
                    new Option(new[] {"-e", "--environment"}, "Name of the environment.")
                    {
                        Argument = new Argument<string>(),
                        Required = false
                    },
                    new Option(new[] {"-v", "--vertical"}, "Name of the vertical.")
                    {
                        Argument = new Argument<string>(),
                        Required = false
                    },
                    new Option(new[] {"-c", "--cluster"}, "Name of the cluster.")
                    {
                        Argument = new Argument<string>(),
                        Required = false
                    },
                    new Option(new[] {"-s", "--subvertical"}, "Name of the sub-vertical (if used).")
                    {
                        Argument = new Argument<string>(),
                        Required = false
                    },
                    new Option(new[] {"-n", "--namespace"}, "Name of namespace.")
                    {
                        Argument = new Argument<string>(),
                        Required = false
                    },
                    new Option(new[] {"--name"}, "Name of the release.")
                    {
                        Argument = new Argument<string>(),
                        Required = false
                    },
                    new Option(new[] {"--renderer"}, "Renderer to use to generate the chart.")
                    {
                        Argument = new Argument<string>() { },
                        Required = false
                    },
                };

            static Command ListEnvironmentsCommand() => new BaseCommand<ListConfigurationsCommandHandler>(
                "list-configurations",
                alias: "ls",
                description: "List all configurations present in the repository."
            );

            static Command DiagnosticsCommand() => new BaseCommand<DiagnosticsCommandHandler>(
                name: "diagnostics",
                alias: "diag",
                description: "Output diagnostic info about the deployment config."
            )
            {
                new Option(new[] {"-e", "--environment"}, "Name of the environment.")
                {
                    Argument = new Argument<string>(),
                    Required = false
                },
                new Option(new[] {"-v", "--vertical"}, "Name of the vertical.")
                {
                    Argument = new Argument<string>(),
                    Required = false
                },
                new Option(new[] {"-c", "--cluster"}, "Name of the cluster.")
                {
                    Argument = new Argument<string>(),
                    Required = false
                },
                new Option(new[] {"-s", "--subvertical"}, "Name of the sub-vertical (if used).")
                {
                    Argument = new Argument<string>(),
                    Required = false
                },
                new Option(new[] {"-n", "--namespace"}, "Name of namespace.")
                {
                    Argument = new Argument<string>(),
                    Required = false
                },
                new Option(new[] {"--name"}, "Name of the release.")
                {
                    Argument = new Argument<string>(),
                    Required = false
                },
                new Option(new[] {"--renderer"}, "Renderer to use to generate the chart.")
                {
                    Argument = new Argument<string>() { },
                    Required = false
                },
            };

            var commandLineBuilder = new CommandLineBuilder()
                .AddCommand(ListEnvironmentsCommand())
                .AddCommand(RenderEnvironment())
                .AddCommand(InformationCommand())
                .AddCommand(DiagnosticsCommand())
                .AddOption(new Option(new[] {"--verbose"}))
                .UseDefaults()
                .UseHost(
                    extraCliArguments => new HostBuilder()
                        .UseDefaultServiceProvider((context, options) => { })
                        .ConfigureLogging((context, builder) =>
                        {
                            builder.AddConsole(options =>
                            {
                                options.DisableColors = false;
                                options.Format = ConsoleLoggerFormat.Default;
                            });
                        })
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
                                services.AddScoped<RenderCommandHandler>();
                                services.AddScoped<ListConfigurationsCommandHandler>();
                                services.AddScoped<InformationCommandHandler>();
                                services.AddScoped<DiagnosticsCommandHandler>();

                                services
                                    .AddScoped<IDeploymentConfigurationPathProvider, DeploymentConfigurationPathProvider
                                    >();
                                services.AddScoped<IDeploymentConfigurationProvider, DeploymentConfigurationProvider>();

                                services.AddScoped<ISecretsHandler, SopsSecretsHandler>();
                                
                                services.Configure<RenderConfiguration>(hostContext.Configuration);
                                
                                services.Configure<ArgoCdEnvironment>(hostContext.Configuration);

                                services.AddScoped<Helm2DeploymentRenderer>();
                                services.AddScoped<Helm3DeploymentRenderer>();
                                services.AddSingleton<IDeploymentRendererFactory, DeploymentRendererFactory>();
                                
                                services.AddScoped<IDeploymentRendererContextBuilder, DeploymentRendererContextBuilder>();
                                
                                services.AddOptions<RenderArguments>().BindCommandLine();
                                services.AddOptions<GeneralArguments>().BindCommandLine();

                            });
                    }
                );

            var cli = commandLineBuilder.Build();
            var parseResults = cli.Parse(args);
            return await parseResults.InvokeAsync();
            
        }

    }
}
