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
            var builder = Host.CreateDefaultBuilder()
                .ConfigureRocketSurgey(rb => rb
                .UseScanner(AutoFake.Resolve<IConventionScanner>())
            .PrependDelegate(new Action(() => { }))
            .AppendDelegate(new Action(() => { })))
            .ConfigureServices((context, collection) => { });
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().PrependDelegate(A<Delegate>._)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().AppendDelegate(A<Delegate>._)).MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public void Should_Call_Through_To_Convention_Methods()
        {
            AutoFake.Provide(new string[0]);
            var convention = AutoFake.Resolve<IConvention>();
            var builder = Host.CreateDefaultBuilder()
                .ConfigureRocketSurgey(rb => rb
                .UseScanner(AutoFake.Resolve<IConventionScanner>())
                .PrependConvention(convention)
                .AppendConvention(convention));
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().PrependConvention(A<IConvention>._)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().AppendConvention(A<IConvention>._)).MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public void Should_Build_The_Host_Correctly()
        {
            var serviceConventionFake = A.Fake<IServiceConvention>();
            var configurationConventionFake = A.Fake<IConfigurationConvention>();
            var commandLineConventionFake = A.Fake<ICommandLineConvention>();

            var builder = Host.CreateDefaultBuilder()
                .ConfigureRocketSurgey(rb => rb
                .UseScanner(new BasicConventionScanner(
                    serviceConventionFake, configurationConventionFake, commandLineConventionFake))
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly })));

            var host = builder.Build();
            host.Services.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_Run_Rocket_CommandLine()
        {
            var builder = Host.CreateDefaultBuilder(Array.Empty<string>())
                .ConfigureRocketSurgey(rb => rb
                .UseScanner(new BasicConventionScanner())
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .AppendDelegate(new CommandLineConventionDelegate(c => c.OnRun(state => 1337)), new CommandLineConventionDelegate(c => c.OnRun(state => 1337))));

            (await builder.RunCli()).Should().Be(1337);
        }

        [Fact]
        public async Task Should_Integrate_With_Autofac()
        {
            var builder = Host.CreateDefaultBuilder(Array.Empty<string>())
                .ConfigureRocketSurgey(rb => rb
                .UseAutofac()
                .UseScanner(new BasicConventionScanner())
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .AppendDelegate(new CommandLineConventionDelegate(c => c.OnRun(state => 1337)), new CommandLineConventionDelegate(c => c.OnRun(state => 1337))));

            (await builder.RunCli()).Should().Be(1337);
        }

        [Fact]
        public async Task Should_Inject_WebHost_Into_Command()
        {
            var builder = Host.CreateDefaultBuilder(new[] { "myself" })
                .ConfigureRocketSurgey(rb => rb
                .UseScanner(new BasicConventionScanner())
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .AppendDelegate(new CommandLineConventionDelegate(c => c.OnRun(state => 1337)))
                .AppendDelegate(new CommandLineConventionDelegate(context => context.AddCommand<MyCommand>("myself"))));

            (await builder.RunCli()).Should().Be(1234);
        }
    }
}
