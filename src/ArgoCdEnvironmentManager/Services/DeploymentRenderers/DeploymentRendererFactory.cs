using System;
using System.Collections.Generic;
using System.Linq;
using HelmPreprocessor.Commands.Arguments;
using HelmPreprocessor.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Services.DeploymentRenderers
{
    class DeploymentRendererFactory : IDeploymentRendererFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly IOptions<RenderArguments> _renderArguments;
        private readonly IDeploymentConfigurationProvider _deploymentConfigurationProvider;
        private readonly Dictionary<string, RendererType> _deploymentRendererTypeMap;

        public DeploymentRendererFactory(
            IServiceProvider serviceProvider,
            IOptions<RenderArguments> renderArguments,
            IDeploymentConfigurationProvider deploymentConfigurationProvider
        )
        {
            _serviceProvider = serviceProvider;
            _renderArguments = renderArguments;
            _deploymentConfigurationProvider = deploymentConfigurationProvider;

            var values = (RendererType[])Enum.GetValues(typeof(RendererType)) ?? new RendererType[] {};
            _deploymentRendererTypeMap = values!
                .ToDictionary(
                    x => Enum.GetName(typeof(RendererType), x)?.ToLower() ?? x.ToString(),
                    x => x
                );
        }

        public RendererType GetDeploymentRendererType()
        {
            var rendererType = RendererType.Helm3;

            if (!string.IsNullOrEmpty(_renderArguments.Value.Renderer))
            {
                // renderer supplied from command line (overrides the one in config)
                rendererType = _deploymentRendererTypeMap[_renderArguments.Value.Renderer.ToLower()];
            }
            else if (_deploymentConfigurationProvider.GetDeploymentConfiguration(out var deploymentConfiguration))
            {
                rendererType = deploymentConfiguration.Renderer.Type;
            }

            return rendererType;
        }

        public IDeploymentRenderer GetDeploymentRenderer()
        {
            return GetDeploymentRendererType() switch
            {
                RendererType.Helm2 => _serviceProvider.GetService<Helm2DeploymentRenderer>(),
                RendererType.Helm3 => _serviceProvider.GetService<Helm3DeploymentRenderer>(),
                _ => throw new InvalidOperationException("An unsupported deployment render was provided.")
            };
        }
    }
}