using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Hosting.Tests
{
    public class RocketHostBuilderTests : AutoTestBase
    {
        public RocketHostBuilderTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void Should_Call_Through_To_Delegate_Methods()
        {
            AutoFake.Provide(new string[0]);
            var builder = RocketHost.CreateDefaultBuilder()
                .UseConventional()
                .UseScanner(AutoFake.Resolve<IConventionScanner>());
            builder.PrependDelegate(new Action(() => { }));
            builder.AppendDelegate(new Action(() => { }));
            builder.ConfigureServices((context, collection) => { });
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().PrependDelegate(A<Delegate>._)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().AppendDelegate(A<Delegate>._)).MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public void Should_Call_Through_To_Convention_Methods()
        {
            AutoFake.Provide(new string[0]);
            var builder = RocketHost.CreateDefaultBuilder()
                .UseConventional()
                .UseScanner(AutoFake.Resolve<IConventionScanner>());
            var convention = AutoFake.Resolve<IConvention>();
            builder.PrependConvention(convention);
            builder.AppendConvention(convention);
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().PrependConvention(A<IConvention>._)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().AppendConvention(A<IConvention>._)).MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public void Should_Build_The_Host_Correctly()
        {
            var serviceConventionFake = A.Fake<IServiceConvention>();
            var configurationConventionFake = A.Fake<IConfigurationConvention>();
            var commandLineConventionFake = A.Fake<ICommandLineConvention>();

            var builder = RocketHost.CreateDefaultBuilder()
                .UseConventional()
                .UseScanner(new BasicConventionScanner(
                    serviceConventionFake, configurationConventionFake, commandLineConventionFake))
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly }));

            var host = builder.Build();
            host.Services.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_Run_Rocket_CommandLine()
        {
            var builder = RocketHost.CreateDefaultBuilder(Array.Empty<string>())
                .UseConventional()
                .UseScanner(new BasicConventionScanner())
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .AppendDelegate(new CommandLineConventionDelegate(c => c.OnRun(state => 1337)), new CommandLineConventionDelegate(c => c.OnRun(state => 1337)));

            (await builder.RunCli()).Should().Be(1337);
        }

        [Fact]
        public async Task Should_Inject_WebHost_Into_Command()
        {
            var builder = RocketHost.CreateDefaultBuilder(new[] { "myself" })
                .UseConventional()
                .UseScanner(new BasicConventionScanner())
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .AppendDelegate(new CommandLineConventionDelegate(c => c.OnRun(state => 1337)))
                .AppendDelegate(new CommandLineConventionDelegate(context => context.AddCommand<MyCommand>("myself")));

            (await builder.RunCli()).Should().Be(1234);
        }
    }
}
