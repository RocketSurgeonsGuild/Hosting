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
        public void Builder_Should_Be_Castable_ToHostBuilder()
        {
            AutoFake.Provide(new string[0]);
            IRocketHostBuilder builder = AutoFake.Resolve<RocketHostBuilder>();
            builder.AsHostBuilder().Should().BeAssignableTo<IHostBuilder>();
        }

        [Fact]
        public void Should_Call_Through_To_Delegate_Methods()
        {
            AutoFake.Provide(new string[0]);
            IRocketHostBuilder builder = AutoFake.Resolve<RocketHostBuilder>();
            builder.PrependDelegate(new Action(() => { }));
            builder.AppendDelegate(new Action(() => { }));
            builder.ConfigureServices((context, collection) => { });
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().PrependDelegate(A<Delegate>._)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().AppendDelegate(A<Delegate>._)).MustHaveHappened(2, Times.Exactly);
        }

        [Fact]
        public void Should_Call_Through_To_Convention_Methods()
        {
            AutoFake.Provide(new string[0]);
            IRocketHostBuilder builder = AutoFake.Resolve<RocketHostBuilder>();
            var convention = AutoFake.Resolve<IConvention>();
            builder.PrependConvention(convention);
            builder.AppendConvention(convention);
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().PrependConvention(A<IConvention>._)).MustHaveHappened(2, Times.Exactly);
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().AppendConvention(A<IConvention>._)).MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public void Should_Build_The_Host_Correctly()
        {
            AutoFake.Provide(new string[0]);
            var serviceConventionFake = A.Fake<IServiceConvention>();
            var configurationConventionFake = A.Fake<IConfigurationConvention>();
            var commandLineConventionFake = A.Fake<ICommandLineConvention>();
            AutoFake.Provide<IHostBuilder>(new HostBuilder());
            AutoFake.Provide<IConventionScanner>(new BasicConventionScanner(
                serviceConventionFake, configurationConventionFake, commandLineConventionFake
            ));
            AutoFake.Provide<IAssemblyCandidateFinder>(
                new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }));
            AutoFake.Provide<IAssemblyProvider>(
                new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly }));

            IRocketHostBuilder builder = AutoFake.Resolve<RocketHostBuilder>();

            var host = builder.Build();
            host.Start();
        }

        [Fact]
        public async Task Should_Run_Rocket_CommandLine()
        {
            AutoFake.Provide(new string[0]);
            AutoFake.Provide<IHostBuilder>(new HostBuilder());
            AutoFake.Provide<IConventionScanner>(new BasicConventionScanner());
            AutoFake.Provide<IAssemblyCandidateFinder>(
                new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }));
            AutoFake.Provide<IAssemblyProvider>(
                new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly }));
            AutoFake.Provide(Array.Empty<string>());

            IRocketHostBuilder builder = AutoFake.Resolve<RocketHostBuilder>();

            var result = builder
                .ContributeCommandLine(c => c.OnRun(state => 1337));

            (await result.GoAsync()).Should().Be(1337);
        }

        [Fact]
        public async Task Should_Inject_WebHost_Into_Command()
        {
            AutoFake.Provide(new string[0]);
            AutoFake.Provide<IHostBuilder>(new HostBuilder());
            AutoFake.Provide<IConventionScanner>(new BasicConventionScanner());
            AutoFake.Provide<IAssemblyCandidateFinder>(
                new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }));
            AutoFake.Provide<IAssemblyProvider>(
                new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly }));
            AutoFake.Provide(new [] { "myself" });

            IRocketHostBuilder builder = AutoFake.Resolve<RocketHostBuilder>();
            var result = builder
                .ContributeCommandLine(c => c.OnRun(state => 1337))
                .AppendDelegate(new CommandLineConventionDelegate(context => context.AddCommand<MyCommand>("myself")));

            (await result.GoAsync()).Should().Be(1234);
        }
    }
}
