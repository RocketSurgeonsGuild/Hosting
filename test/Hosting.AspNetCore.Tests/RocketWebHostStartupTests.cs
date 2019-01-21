using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Rocket.Surgery.AspNetCore.Hosting;
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
        private readonly IRocketWebHostBuilder _baseBuilder;
        public RocketWebHostStartupTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _baseBuilder = WebHost.CreateDefaultBuilder()
                .UseConventional()
                .UseScanner(new BasicConventionScanner())
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketWebHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketWebHostBuilderTests).Assembly }));
        }

        [Fact]
        public async Task Should_Start_Application()
        {
            var builder = _baseBuilder;
            var result = builder;
            builder.UseStartup<SimpleStartup>();

            using (var server = new TestServer(builder))
            {
                var response = await server.CreateRequest("/")
                    .GetAsync();

                var content = await response.Content.ReadAsStringAsync();
                content.Should().Be("SimpleStartup -> Configure");
            }
        }
    }
}
