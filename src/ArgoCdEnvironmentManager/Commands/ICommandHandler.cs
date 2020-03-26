using System.Threading;
using System.Threading.Tasks;

namespace HelmPreprocessor.Commands
{
    public interface ICommandHandler
    {
        Task Run(CancellationToken cancellationToken);
    }
}