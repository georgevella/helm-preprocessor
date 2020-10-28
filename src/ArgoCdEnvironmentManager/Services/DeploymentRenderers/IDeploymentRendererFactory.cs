using HelmPreprocessor.Configuration;

namespace HelmPreprocessor.Services.DeploymentRenderers
{
    public interface IDeploymentRendererFactory
    {
        RendererType GetDeploymentRendererType();
        
        IDeploymentRenderer GetDeploymentRenderer();
    }
}