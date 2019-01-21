using System;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
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
            var host = WebHost.CreateDefaultBuilder().LaunchWith(RocketBooster.For(AppDomain.CurrentDomain));
            host.Should().BeAssignableTo<IRocketWebHostBuilder>();
        }

        [Fact]
        public void Creates_RocketHost_ForAssemblies()
        {
            var host = WebHost.CreateDefaultBuilder().LaunchWith(RocketBooster.For(new[] { typeof(RocketWebHostTests).Assembly }));
            host.Should().BeAssignableTo<IRocketWebHostBuilder>();
        }

        [Fact]
        public void Creates_RocketHost_ForDependencyContext()
        {
            var host = WebHost.CreateDefaultBuilder().LaunchWith(RocketBooster.For(DependencyContext.Load(typeof(RocketWebHostTests).Assembly)));
            host.Should().BeAssignableTo<IRocketWebHostBuilder>();
        }
    }
}
