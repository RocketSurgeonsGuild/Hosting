using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Hosting.AspNetCore.Tests.Startups;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests
{
    public class RocketWebHostBuilderTests : AutoTestBase
    {
        public RocketWebHostBuilderTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void Should_Call_Through_To_Delegate_Methods()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(x => { })
                .ConfigureRocketSurgery(x => x
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
            var convention = AutoFake.Resolve<IConvention>();
            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(x => { })
                .ConfigureRocketSurgery(x => x
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

            var builder = Host.CreateDefaultBuilder(Array.Empty<string>())
                .ConfigureWebHostDefaults(x => x.UseStartup<TestStartup>().UseTestServer())
                .ConfigureRocketSurgery(x => x
                .UseScanner(new BasicConventionScanner(A.Fake<IServiceProviderDictionary>(),
                    serviceConventionFake, configurationConventionFake
                ))
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketWebHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketWebHostBuilderTests).Assembly }))
                );
            using (var host = builder.Build())
            {
                host.StartAsync();
                var server = host.GetTestServer();
                server.CreateClient();
                host.StopAsync();
            }
        }

        [Fact]
        public async Task Should_Run_The_Cli()
        {
            var serviceConventionFake = A.Fake<IServiceConvention>();
            var configurationConventionFake = A.Fake<IConfigurationConvention>();

            var builder = Host.CreateDefaultBuilder(new[] { "myself" })
                .ConfigureWebHostDefaults(x => x.UseStartup<MyStartup>().UseTestServer())
                .ConfigureRocketSurgery(x => x
                .UseScanner(new BasicConventionScanner(A.Fake<IServiceProviderDictionary>(),
                    serviceConventionFake, configurationConventionFake
                ))
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketWebHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketWebHostBuilderTests).Assembly }))
                .AppendDelegate(new CommandLineConventionDelegate(c => c.OnRun(state => 1337)))
                .AppendDelegate(new CommandLineConventionDelegate(context => context.AddCommand<MyCommand>("myself", v => { })))
                );

            var result = await builder.RunCli();
            result.Should().Be(1234);
        }
    }
}
