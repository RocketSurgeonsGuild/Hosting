using System;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Rocket.Surgery.Hosting.Functions.Tests
{
    public class RocketHostTests
    {
        [Fact]
        public void Creates_RocketHost_ForAppDomain()
        {
            var host = new WebJobsBuilder().UseRocketBooster(new Startup(), RocketBooster.For(AppDomain.CurrentDomain), rb => { });
            host.Should().BeAssignableTo<IWebJobsBuilder>();
        }

        [Fact]
        public void Creates_RocketHost_ForAssemblies()
        {
            var host = new WebJobsBuilder().UseRocketBooster(new Startup(), RocketBooster.For(new[] { typeof(RocketHostTests).Assembly }),
                rb => { });
            host.Should().BeAssignableTo<IWebJobsBuilder>();
        }
    }
}
