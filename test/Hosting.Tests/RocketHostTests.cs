using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Xunit;

namespace Rocket.Surgery.Hosting.Tests
{
    public class RocketHostTests
    {
        [Fact]
        public void Creates_RocketHost_ForAppDomain()
        {
            var host = RocketHost.ForAppDomain(AppDomain.CurrentDomain);
            host.Should().BeOfType<RocketHostBuilder>();
        }

        [Fact]
        public void Creates_RocketHost_ForAssemblies()
        {
            var host = RocketHost.ForAssemblies(new[] { typeof(RocketHostTests).Assembly });
            host.Should().BeOfType<RocketHostBuilder>();
        }

        [Fact]
        public void Creates_RocketHost_ForDependencyContext()
        {
            var host = RocketHost.ForDependencyContext(DependencyContext.Load(typeof(RocketHostTests).Assembly));
            host.Should().BeOfType<RocketHostBuilder>();
        }
    }
}
