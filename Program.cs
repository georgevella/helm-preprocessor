using System;
using System.Collections;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using HelmPreprocessor.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HelmPreprocessor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var worker = host.Services.GetService<Worker>();
            
            await worker.StartAsync(CancellationToken.None);
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables(prefix: "HELM_");
                    builder.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    var hostConfiguration = hostContext.Configuration.Get<HostConfiguration>();

                    var configurationRoot = hostConfiguration.Configuration;

                    if (string.IsNullOrWhiteSpace(configurationRoot))
                    {
                        configurationRoot = Path.Combine(
                            hostConfiguration.Repository ?? Environment.CurrentDirectory,
                            "config",
                            $"{hostConfiguration.Cluster}-{hostConfiguration.Environment}",
                            hostConfiguration.SubVertical
                        );
                    }

                    builder.AddYamlFile(Path.Combine(configurationRoot, "preprocessor.yaml"));
                    builder.AddYamlFile(Path.Combine(configurationRoot, "values.yaml"));
                    builder.AddYamlFile(Path.Combine(configurationRoot, "app-versions.yaml"));
                    builder.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    //services.AddHostedService<Worker>();
                    services.AddSingleton<Worker>();
                    services.Configure<HostConfiguration>(hostContext.Configuration);
                    services.ConfigureDictionary<ServicesConfiguration, ServiceConfiguration>(hostContext.Configuration.GetSection("services"));

                });
    }
}
