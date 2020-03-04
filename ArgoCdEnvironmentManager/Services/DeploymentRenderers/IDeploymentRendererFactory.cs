using System;
using System.Collections.Generic;
using System.Linq;

namespace HelmPreprocessor.Services.DeploymentRenderers
{
    public interface IDeploymentRendererFactory
    {
        IDeploymentRenderer GetDeploymentRenderer(string name);
    }

    class DeploymentRendererFactory : IDeploymentRendererFactory
    {
        private readonly Dictionary<string, IDeploymentRenderer> _deploymentRendererMap;

        public DeploymentRendererFactory(IEnumerable<IDeploymentRenderer> deploymentRenderers)
        {
            _deploymentRendererMap = deploymentRenderers
                .Select(x => new
                {
                    name = GenerateNameFromType(x.GetType()),
                    instance = x
                })
                .ToDictionary(x => x.name, x => x.instance);
        }

        private string GenerateNameFromType(Type type)
        {
            var name = type.Name.ToLower();
            if (name.EndsWith("DeploymentRenderer".ToLower()))
            {
                name = name.Substring(0, name.IndexOf("DeploymentRenderer".ToLower(), StringComparison.Ordinal));
            }

            return name;
        }

        public IDeploymentRenderer GetDeploymentRenderer(string name)
        {
            return _deploymentRendererMap[name];
        }
    }
}