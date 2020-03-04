using System;
using System.Collections.Generic;
using System.Diagnostics;
using HelmPreprocessor.Configuration;
using Microsoft.Extensions.Options;

namespace HelmPreprocessor.Services
{
    public interface IDeploymentRenderer
    {
        void Initialize(DeploymentRendererContext context);
        
        void Render(DeploymentRendererContext context);
    }

    public class DeploymentRendererContext
    {
        public string Name { get; set; }
        
        public string Namespace { get; set; }
        
        public string WorkingDirectory { get; set; }
    }

    public class HelmRendererContext : DeploymentRendererContext
    {
        public List<string> ValueFiles { get; set; } = new List<string>();
    }
}