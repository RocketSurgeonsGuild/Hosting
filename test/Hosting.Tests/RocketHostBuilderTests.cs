using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                .ConfigureRocketSurgery(rb => rb
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
                .ConfigureRocketSurgery(rb => rb
                .UseScanner(AutoFake.Resolve<IConventionScanner>())
                .PrependConvention(convention)
                .AppendConvention(convention));
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().PrependConvention(A<IConvention>._)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().AppendConvention(A<IConvention>._)).MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public void Should_UseAppDomain()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureRocketSurgery(rb => rb
                    .UseAppDomain(AppDomain.CurrentDomain)
                );

            var host = builder.Build();
            host.Services.Should().NotBeNull();
        }

        [Fact]
        public void Should_UseAssemblies()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureRocketSurgery(rb => rb
                    .UseAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                );

            var host = builder.Build();
            host.Services.Should().NotBeNull();
        }

        [Fact]
        public void Should_UseRocketBooster()
        {
            var builder = Host.CreateDefaultBuilder()
                .UseRocketBooster(RocketBooster.For(AppDomain.CurrentDomain));

            var host = builder.Build();
            host.Services.Should().NotBeNull();
        }

        [Fact]
        public void Should_UseDiagnosticLogging()
        {
            var builder = Host.CreateDefaultBuilder()
                .UseRocketBooster(RocketBooster.For(AppDomain.CurrentDomain), x => x.UseDiagnosticLogging(c => c.AddConsole()));

            var host = builder.Build();
            host.Services.Should().NotBeNull();
        }

        [Fact]
        public void Should_UseDiagnosticSource()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureRocketSurgery(rb => rb
                    .UseDiagnosticSource(new DiagnosticListener("abcd"))
                    .UseAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                );

            var host = builder.Build();
            host.Services.Should().NotBeNull();
        }

        [Fact]
        public void Should_UseDependencyContext()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureRocketSurgery(rb => rb
                    .UseDependencyContext(DependencyContext.Default)
                );

            var host = builder.Build();
            host.Services.Should().NotBeNull();
        }

        [Fact]
        public void Should_Build_The_Host_Correctly()
        {
            var serviceConventionFake = A.Fake<IServiceConvention>();
            var configurationConventionFake = A.Fake<IConfigurationConvention>();
            var commandLineConventionFake = A.Fake<ICommandLineConvention>();

            var builder = Host.CreateDefaultBuilder()
                .ConfigureRocketSurgery(rb => rb
                    .UseScanner(new BasicConventionScanner(A.Fake<IServiceProviderDictionary>(),
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
                .ConfigureRocketSurgery(rb => rb
                .UseScanner(new BasicConventionScanner(A.Fake<IServiceProviderDictionary>()))
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .AppendDelegate(new CommandLineConventionDelegate(c => c.OnRun(state => 1337)), new CommandLineConventionDelegate(c => c.OnRun(state => 1337))));

            (await builder.RunCli()).Should().Be(1337);
        }

        [Fact]
        public async Task Should_Integrate_With_Autofac()
        {
            var builder = Host.CreateDefaultBuilder(Array.Empty<string>())
                .ConfigureRocketSurgery(rb => rb
                .UseAutofac()
                .UseScanner(new BasicConventionScanner(A.Fake<IServiceProviderDictionary>()))
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .AppendDelegate(new CommandLineConventionDelegate(c => c.OnRun(state => 1337)), new CommandLineConventionDelegate(c => c.OnRun(state => 1337))));

            using (var host = builder.Build())
            {
                await host.StartAsync();
                var container = host.Services.GetRequiredService<ILifetimeScope>();
                container.Should().NotBeNull();
                await host.StopAsync();
            }
        }

        [Fact]
        public async Task Should_Inject_WebHost_Into_Command()
        {
            var builder = Host.CreateDefaultBuilder(new[] { "myself" })
                .ConfigureRocketSurgery(rb => rb
                .UseScanner(new BasicConventionScanner(A.Fake<IServiceProviderDictionary>()))
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly }))
                .AppendDelegate(new CommandLineConventionDelegate(c => c.OnRun(state => 1337)))
                .AppendDelegate(new CommandLineConventionDelegate(context => context.AddCommand<MyCommand>("myself"))));

            (await builder.RunCli()).Should().Be(1234);
        }
    }
}
