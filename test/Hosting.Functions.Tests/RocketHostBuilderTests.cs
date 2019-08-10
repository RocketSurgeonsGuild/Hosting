using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace Rocket.Surgery.Hosting.Functions.Tests
{
    class WebJobsBuilder : IWebJobsBuilder
    {
        private readonly IServiceCollection _services;

        public WebJobsBuilder()
        {
            _services = new ServiceCollection();
            _services.AddSingleton< IConfiguration>(new ConfigurationBuilder().Build());
        }

        public IServiceCollection Services => _services;
    }

    class Startup : IServiceConvention
    {
        public void Register(IServiceConventionContext context)
        {
            context.Services.AddSingleton(context);
        }
    }

    public class RocketHostBuilderTests : AutoTestBase
    {
        public RocketHostBuilderTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void Should_Call_Through_To_Delegate_Methods()
        {
            var startup = new Startup();
            var builder = new WebJobsBuilder()
                .UseRocketBooster(
                    startup,
                    RocketBooster.For(DependencyContext.Load(typeof(RocketHostTests).Assembly)),
                    rb => rb
                        .UseScanner(AutoFake.Resolve<IConventionScanner>())
                        .PrependDelegate(new Action(() => { }))
                        .AppendDelegate(new Action(() => { }))
                    );

            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().PrependDelegate(A<Delegate>._)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().AppendDelegate(A<Delegate>._)).MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public void Should_Call_Through_To_Convention_Methods()
        {
            var convention = AutoFake.Resolve<IConvention>();
            var startup = new Startup();
            var builder = new WebJobsBuilder()
                .UseRocketSurgery(startup, rb => rb
                .UseScanner(AutoFake.Resolve<IConventionScanner>())
                .PrependConvention(convention)
                .AppendConvention(convention)
            );
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().PrependConvention(A<IConvention>._)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().AppendConvention(A<IConvention>._)).MustHaveHappened(1, Times.Exactly);
        }

        [Fact(Skip = "Disabled because it sometimes fails on windows")]
        public void Should_UseAppDomain()
        {
            var startup = new Startup();
            var builder = new WebJobsBuilder()
                .UseRocketSurgery(startup, rb => rb
                    .UseAppDomain(AppDomain.CurrentDomain)
                );

            var sp = builder.Services.BuildServiceProvider();
            sp.Should().NotBeNull();
        }

        [Fact]
        public void Should_UseAssemblies()
        {
            var startup = new Startup();
            var builder = new WebJobsBuilder()
                .UseRocketSurgery(startup, rb => rb
                    .UseAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                );

            var sp = builder.Services.BuildServiceProvider();
            sp.Should().NotBeNull();
        }

        [Fact]
        public void Should_UseRocketBooster()
        {
            var startup = new Startup();
            var builder = new WebJobsBuilder()
                .UseRocketBooster(startup, RocketBooster.For(AppDomain.CurrentDomain), rb => {});

            var sp = builder.Services.BuildServiceProvider();
            sp.Should().NotBeNull();
        }

        [Fact]
        public void Should_UseDependencyContext()
        {
            var startup = new Startup();
            var builder = new WebJobsBuilder()
                .UseRocketSurgery(startup, rb => rb
                    .UseDependencyContext(DependencyContext.Default)
                );

            var sp = builder.Services.BuildServiceProvider();
            sp.Should().NotBeNull();
        }

        [Fact]
        public void Should_Build_The_Host_Correctly()
        {
            var serviceConventionFake = A.Fake<IServiceConvention>();
            var configurationConventionFake = A.Fake<IConfigurationConvention>();

            var startup = new Startup();
            var builder = new WebJobsBuilder()
                .UseRocketSurgery(startup, rb => rb
                    .UseScanner(new BasicConventionScanner(A.Fake<IServiceProviderDictionary>(),
                        serviceConventionFake, configurationConventionFake))
                    .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketHostBuilderTests).Assembly }))
                    .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketHostBuilderTests).Assembly })));

            var sp = builder.Services.BuildServiceProvider();
            sp.Should().NotBeNull();
        }
    }
}
