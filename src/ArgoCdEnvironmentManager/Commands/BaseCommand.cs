using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HelmPreprocessor.Commands
{
    public class BaseCommand<TCommandHandler> : Command
        where TCommandHandler : ICommandHandler
    {
        public BaseCommand(string name, string? alias = null, string? description = null) : base(name, description)
        {
            if (!string.IsNullOrWhiteSpace(alias))
            {
                AddAlias(alias);
            }
            
            Handler = CommandHandler.Create<IHost>( 
                async host =>
                {
                    await host.Services.GetService<TCommandHandler>().Run(CancellationToken.None);
                });
        }
    }
}