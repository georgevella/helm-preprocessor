using System.IO;

namespace HelmPreprocessor.Services
{
    public interface IDeploymentConfigurationPathProvider
    {
        bool TryGetConfigurationRoot(out DirectoryInfo configurationRootDirectory);
    }
}