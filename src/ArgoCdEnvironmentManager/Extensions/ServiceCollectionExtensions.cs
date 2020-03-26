using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelmPreprocessor.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureDictionary<TOptions, TSubOptions>(
            this IServiceCollection services,
            IConfigurationSection section
        ) 
            where TSubOptions : new()
            where TOptions : class, IDictionary<string, TSubOptions>
            
        {
            services.Configure<TOptions>(x =>
            {
                var values = section
                    .GetChildren()
                    .ToList();
                
                values.ForEach(v =>
                {
                    var subOptions = new TSubOptions();
                    v.Bind(subOptions);
                    x.Add(v.Key, subOptions);
                });
            });

            return services;
        }
    }
}