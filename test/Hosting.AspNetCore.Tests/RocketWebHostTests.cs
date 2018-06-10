using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyModel;
using Rocket.Surgery.AspNetCore.Hosting;
using Xunit;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests
{
    public class RocketWebHostTests
    {
        [Fact]
        public void Creates_RocketHost_ForAppDomain()
        {
            var host = RocketWebHost.ForAppDomain(AppDomain.CurrentDomain);
            host.Should().BeOfType<RocketWebHostBuilder>();
        }

        [Fact]
        public void Creates_RocketHost_ForAssemblies()
        {
            var host = RocketWebHost.ForAssemblies(new[] { typeof(RocketWebHostTests).Assembly });
            host.Should().BeOfType<RocketWebHostBuilder>();
        }

        [Fact]
        public void Creates_RocketHost_ForDependencyContext()
        {
            var host = RocketWebHost.ForDependencyContext(DependencyContext.Load(typeof(RocketWebHostTests).Assembly));
            host.Should().BeOfType<RocketWebHostBuilder>();
        }
    }
}
