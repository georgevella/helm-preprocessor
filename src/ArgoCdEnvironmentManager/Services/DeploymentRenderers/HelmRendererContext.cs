using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;

namespace HelmPreprocessor.Services.DeploymentRenderers
{
    public class HelmRendererContext : DeploymentRendererContext
    {
        public HelmChart? Chart { get; }
        
        public List<string> ValueFiles { get; set; } = new List<string>();

        public HelmRendererContext(
            string name, 
            string ns, 
            DirectoryInfo workingDirectory, 
            HelmChart? chart = null, 
            string? cluster = null, 
            string? environment = null, 
            string? vertical = null, 
            string? subVertical = null
            ) 
            : base(name, ns, workingDirectory, cluster, environment, vertical, subVertical)
        {
            Chart = chart;
        }
    }

    public class HelmChart 
    {
        public HelmChart(string name, HelmChartRepository repository, string? version = null)
        {
            Name = name;
            Repository = repository;
            Version = version;
        }

        public string Name { get; }
        
        public string? Version { get; }
        
        public HelmChartRepository Repository { get; }
    }

    public class HelmChartRepository
    {
        public HelmChartRepository(string url)
        {
            Url = url;
        }

        public string Url { get; }
    }

    public abstract class AuthenticatedHelmChartRepository : HelmChartRepository
    {
        protected AuthenticatedHelmChartRepository(string url) : base(url)
        {
        }
    }

    public class UsernamePasswordAuthHelmChartRepository : AuthenticatedHelmChartRepository
    {
        public NetworkCredential Credentials { get; }

        public UsernamePasswordAuthHelmChartRepository(string url, NetworkCredential credentials) 
            : base(url)
        {
            Credentials = credentials;
        }
        
        public UsernamePasswordAuthHelmChartRepository(string url, string username, SecureString password) 
            : this(url, new NetworkCredential(username, password))
        {
            
        }
    }
}