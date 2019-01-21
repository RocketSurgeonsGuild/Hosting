using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Rocket.Surgery.Hosting.Tests
{
    public class RocketHostTests
    {
        [Fact]
        public void Creates_RocketHost_ForAppDomain()
        {
            var host = RocketHost.CreateDefaultBuilder().LaunchWith(RocketBooster.For(AppDomain.CurrentDomain));
            host.Should().BeAssignableTo<IRocketHostBuilder>();
        }

        [Fact]
        public void Creates_RocketHost_ForAssemblies()
        {
            var host = RocketHost.CreateDefaultBuilder().LaunchWith(RocketBooster.For(new[] { typeof(RocketHostTests).Assembly }));
            host.Should().BeAssignableTo<IRocketHostBuilder>();
        }

        [Fact]
        public void Creates_RocketHost_ForDependencyContext()
        {
            var host = RocketHost.CreateDefaultBuilder().LaunchWith(RocketBooster.For(DependencyContext.Load(typeof(RocketHostTests).Assembly)));
            host.Should().BeAssignableTo<IRocketHostBuilder>();
        }
    }
}
