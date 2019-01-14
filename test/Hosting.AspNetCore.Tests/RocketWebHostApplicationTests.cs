using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Rocket.Surgery.AspNetCore.Hosting;
using Rocket.Surgery.AspNetCore.Hosting.Cli;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Hosting.AspNetCore.Tests.Startups;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests
{
    public class RocketWebHostApplicationTests : AutoTestBase
    {
        public RocketWebHostApplicationTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            AutoFake.Provide(WebHost.CreateDefaultBuilder());
            AutoFake.Provide<IConventionScanner>(new BasicConventionScanner());
            AutoFake.Provide<IAssemblyCandidateFinder>(
                new DefaultAssemblyCandidateFinder(new[] { typeof(RocketWebHostBuilderTests).Assembly }));
            AutoFake.Provide<IAssemblyProvider>(
                new DefaultAssemblyProvider(new[] { typeof(RocketWebHostBuilderTests).Assembly }));
        }


        [Fact]
        public async Task Should_Start_System_Based_Application()
        {
            AutoFake.Provide(new string[0]);
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            var result = builder
                .ContributeCommandLine(c => c.OnRun(state => 1337));
            builder.UseStartup<TestStartup>();

            (await result.GoAsync()).Should().Be(1337);
        }

        [Fact]
        public async Task Should_Run_Command_Given_Arguments1()
        {
            AutoFake.Provide(new string[] {"dosomething"});
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            var result = builder
                .ContributeCommandLine(c => c.OnRun(state => 1337))
                .ContributeConvention(new CommandLineConvention());
            builder.UseStartup<TestStartup>();

            (await result.GoAsync()).Should().Be(1001);
        }

        [Fact]
        public async Task Should_Start_Application()
        {
            AutoFake.Provide(new string[0]);
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            var result = builder
                .ContributeCommandLine(c => c.OnRun(state => 1337));
            builder.UseStartup<TestStartup>();

            using (var server = new TestServer(builder))
            {
                var response = await server.CreateRequest("/")
                    .GetAsync();

                var content = await response.Content.ReadAsStringAsync();
                content.Should().Be("TestStartup -> Compose");
            }
        }
    }
}
