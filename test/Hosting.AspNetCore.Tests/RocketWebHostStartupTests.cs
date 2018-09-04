using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Rocket.Surgery.AspNetCore.Hosting;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Hosting.AspNetCore.Tests.Startups;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests
{
    public class RocketWebHostStartupTests : AutoTestBase
    {
        public RocketWebHostStartupTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            AutoFake.Provide(WebHost.CreateDefaultBuilder());
            AutoFake.Provide<IConventionScanner>(new BasicConventionScanner());
            AutoFake.Provide<IAssemblyCandidateFinder>(
                new DefaultAssemblyCandidateFinder(new[] { typeof(RocketWebHostBuilderTests).Assembly }));
            AutoFake.Provide<IAssemblyProvider>(
                new DefaultAssemblyProvider(new[] { typeof(RocketWebHostBuilderTests).Assembly }));
        }


        [Fact]
        public async Task Should_Start_Simple_Application()
        {
            AutoFake.Provide(new string[0]);
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            var result = await builder
                .ContributeCommandLine(c => c.OnRun(state => 1337))
                .UseStartup<SimpleStartup>()
                .GoAsync();

            result.Should().Be(1337);
        }

        [Fact]
        public async Task Should_Run_Command_Given_Arguments1()
        {
            AutoFake.Provide(new string[] { "dosomething" });
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            var result = builder
                .AppendConvention(new CommandLineConvention())
                .ContributeCommandLine(c => c.OnRun(state => 1337));
            builder.UseStartup<SimpleStartup>();

            (await result.GoAsync()).Should().Be(1001);
        }

        [Fact]
        public async Task Should_Start_Application()
        {
            AutoFake.Provide(new string[] { });
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            var result = builder
                .ContributeCommandLine(c => c.OnRun(state => 1337));
            builder.UseStartup<SimpleStartup>();

            using (var server = new TestServer(builder))
            {
                var response = await server.CreateRequest("/")
                    .GetAsync();

                var content = await response.Content.ReadAsStringAsync();
                content.Should().Be("SimpleStartup -> Configure");
            }
        }

        [Fact]
        public async Task Should_Inject_WebHost_Into_Command()
        {
            AutoFake.Provide(new string[] { "myself" });
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            var result = builder
                .ContributeCommandLine(c => c.OnRun(state => 1337))
                .AppendDelegate(new CommandLineConventionDelegate(context => context.AddCommand<MyCommand>("myself")))
                .UseStartup<SimpleStartup>();

            (await builder.GoAsync()).Should().Be(1234);
        }
    }
}
