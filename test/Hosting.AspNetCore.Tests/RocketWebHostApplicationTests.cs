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

            Builder = AutoFake.Resolve<RocketWebHostBuilder>();
        }

        public IRocketWebHostBuilder Builder { get; }


        [Fact]
        public async Task Should_Start_System_Based_Application()
        {
            var result = Builder
                .UseCli(new string[] { }, x => x.OnRun(state => 1337));
            Builder.UseStartup<TestStartup>();

            (await result.RunCli()).Should().Be(1337);
        }

        [Fact]
        public async Task Should_Run_Command_Given_Arguments1()
        {
            var result = Builder
                .AppendConvention(new CommandLineConvention())
                .UseCli(new string[] { "dosomething" }, x => x.OnRun(state => 1337));
            Builder.UseStartup<TestStartup>();

            (await result.RunCli()).Should().Be(1001);
        }

        [Fact]
        public async Task Should_Start_Application()
        {
            var result = Builder
                .UseCli(new string[] { }, x => x.OnRun(state => 1337));
            Builder.UseStartup<TestStartup>();

            using (var server = new TestServer(Builder))
            {
                var response = await server.CreateRequest("/")
                    .GetAsync();

                var content = await response.Content.ReadAsStringAsync();
                content.Should().Be("TestStartup -> Compose");
            }
        }
    }
}