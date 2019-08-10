using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Hosting.AspNetCore.Tests.Startups;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests
{
    public class RocketWebHostStartupTests : AutoTestBase
    {
        private readonly IHostBuilder _baseBuilder;

        public RocketWebHostStartupTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _baseBuilder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(x => { })
                .ConfigureRocketSurgery(x => x
                .UseScanner(new BasicConventionScanner(A.Fake<IServiceProviderDictionary>()))
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketWebHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketWebHostBuilderTests).Assembly })));
        }

        [Fact]
        public async Task Should_Start_Application()
        {
            var builder = _baseBuilder;
            var result = builder;
            builder.ConfigureWebHost(x => x.UseStartup<SimpleStartup>().UseTestServer());

            using (var host = builder.Build())
            {
                await host.StartAsync();
                var server = host.GetTestServer();
                var response = await server.CreateRequest("/")
                    .GetAsync();

                response.Content.Headers.Allow.Count.ToString();
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Be("SimpleStartup -> Configure");
                await host.StopAsync();
            }
        }
    }
}
