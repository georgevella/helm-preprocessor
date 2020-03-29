using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HelmPreprocessor.Commands.Arguments;
using HelmPreprocessor.Configuration;
using HelmPreprocessor.Services;
using HelmPreprocessor.Services.DeploymentRenderers;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Commands.Handlers
{
    public class RenderCommandHandler : ICommandHandler
    {
        private readonly IDeploymentRendererFactory _deploymentRendererFactory;
        private readonly IDeploymentRendererContextBuilder _deploymentRendererContextBuilder;

        public RenderCommandHandler(
            IDeploymentRendererFactory deploymentRendererFactory,
            IDeploymentRendererContextBuilder deploymentRendererContextBuilder
        )
        {
            _deploymentRendererFactory = deploymentRendererFactory;
            _deploymentRendererContextBuilder = deploymentRendererContextBuilder;
        }
        
        public Task Run(CancellationToken cancellationToken)
        {
            var deploymentContext = _deploymentRendererContextBuilder.GenerateDeploymentRendererContext();
            var deploymentRenderer = _deploymentRendererFactory.GetDeploymentRenderer();
            
            deploymentRenderer.Initialize(deploymentContext);
            deploymentRenderer.Render(deploymentContext);

            return Task.CompletedTask;
        }
    }
}