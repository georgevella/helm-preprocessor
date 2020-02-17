using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using HelmPreprocessor.Configuration;
using HelmPreprocessor.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ArgoCdEnvironmentManager.Tests
{
    public class DeploymentConfigurationPathProviderTests
    {
        [Theory]
        [MemberData(nameof(Data))]
        public void VerifyConfigurationRootPathWithAllVariablesDefined(string cluster, string environment, string vertical, string subvertical, string expectedPath)
        {
            // setup
            var renderConfigurationMock = new Mock<IOptions<RenderConfiguration>>();
            var renderArgumentsMock = new Mock<IOptions<RenderArguments>>();

            renderArgumentsMock.SetupGet(x => x.Value).Returns(new RenderArguments());

            renderConfigurationMock.SetupGet(x => x.Value).Returns(
                new RenderConfiguration()
                {
                    Repository = Environment.CurrentDirectory,
                    Cluster = cluster,
                    Environment = environment,
                    Vertical = vertical,
                    SubVertical = subvertical
                }
            );

            var p = new DeploymentConfigurationPathProvider(
                renderConfigurationMock.Object,
                renderArgumentsMock.Object
                );


            p.TryGetConfigurationRoot(out var configurationRootDirectory).Should().BeTrue();

            configurationRootDirectory.FullName.Should().Be(expectedPath);
        }
        
        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { 
                    "rvr", 
                    "dev", 
                    "platform", 
                    "admin", 
                    Path.Combine(
                    Environment.CurrentDirectory, 
                    "config", 
                    "platform", 
                    "rvr-dev", 
                    "admin") 
                },
                new object[] { 
                    "rvr", 
                    "dev", 
                    null, 
                    "admin", 
                    Path.Combine(
                    Environment.CurrentDirectory, 
                    "config", 
                    "rvr-dev", 
                    "admin") 
                },               
                 new object[] { 
                    null, 
                    null, 
                    null, 
                    "admin", 
                    Path.Combine(
                    Environment.CurrentDirectory, 
                    "config", 
                    "admin") 
                },
            };
    }
}
