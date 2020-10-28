namespace HelmPreprocessor.Services.DeploymentRenderers
{
    public interface IDeploymentRendererFactory
    {
        IDeploymentRenderer GetDeploymentRenderer();
    }
}