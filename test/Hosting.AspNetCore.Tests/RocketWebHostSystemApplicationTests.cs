using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Rocket.Surgery.AspNetCore.Hosting;
using Rocket.Surgery.AspNetCore.Hosting.Cli;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Hosting.AspNetCore.Tests.Cli;
using Rocket.Surgery.Hosting.AspNetCore.Tests.Startups;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests
{
    public class RocketWebHostSystemApplicationTests : AutoTestBase
    {
        public RocketWebHostSystemApplicationTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            AutoFake.Provide(WebHost.CreateDefaultBuilder());
            var source = new DiagnosticListener("Test");
            source.SubscribeWithAdapter(new DiagnosticListenerLoggingAdapter(Logger));
            AutoFake.Provide<DiagnosticSource>(source);
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
            builder.UseStartup<TestApplicationStartup>();

            (await result.GoAsync()).Should().Be(1337);
        }

        [Fact]
        public async Task Should_Run_Command_Given_Arguments1()
        {
            AutoFake.Provide(new [] { "dosomething" });
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            var result = builder
                .AppendConvention(new CommandLineConvention())
                .ContributeCommandLine(c => c.OnRun(state => 1337));
            builder.UseStartup<TestApplicationStartup>();

            (await result.GoAsync()).Should().Be(1001);
        }

        [Fact]
        public async Task Should_Start_Application()
        {
            AutoFake.Provide(new string[0]);
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            var result = builder
                .ContributeCommandLine(c => c.OnRun(state => 1337));
            builder.UseStartup<TestApplicationStartup>();

            using (var server = new TestServer(builder))
            {
                var response = await server.CreateRequest("/")
                    .GetAsync();

                var content = await response.Content.ReadAsStringAsync();
                content.Should().Be("TestApplicationStartup -> Compose");
            }
        }

        [Fact]
        public async Task Should_Start_System()
        {
            AutoFake.Provide(new string[0]);
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            var result = builder
                .ContributeCommandLine(c => c.OnRun(state => 1337));
            builder.UseStartup<TestApplicationStartup>();

            using (var server = new TestServer(builder))
            {
                var response = await server.CreateRequest("/system")
                    .GetAsync();

                var content = await response.Content.ReadAsStringAsync();
                content.Should().Be("TestApplicationStartup -> ComposeSystem");
            }
        }

        [Fact]
        public async Task Should_Inject_WebHost_Into_Command()
        {
            AutoFake.Provide(new[] { "myself" });
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            var result = builder
                .ContributeCommandLine(c => c.OnRun(state => 1337))
                .AppendDelegate(new CommandLineConventionDelegate(context => context.AddCommand<MyCommand>("myself")))
                .UseStartup<SimpleStartup>();

            (await builder.GoAsync()).Should().Be(1234);
        }
    }
}
