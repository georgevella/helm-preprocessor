using System.IO;
using System.Threading.Tasks;

namespace HelmPreprocessor.Services
{
    public interface ISecretsHandler
    {
        public Task<FileInfo> DecodeAsync(FileInfo fileInfo);
        FileInfo Decode(FileInfo fileInfo);
    }
}