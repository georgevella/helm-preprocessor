using System.Threading;
using System.Threading.Tasks;

namespace HelmPreprocessor.Commands
{
    public interface ICommandHandlerService
    {
        Task Run(CancellationToken cancellationToken);
    }
}