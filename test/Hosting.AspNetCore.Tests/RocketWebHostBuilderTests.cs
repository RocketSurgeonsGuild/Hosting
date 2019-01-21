using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.AspNetCore.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
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
            var builder = WebHost.CreateDefaultBuilder()
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
            var builder = WebHost.CreateDefaultBuilder()
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

            var builder = WebHost.CreateDefaultBuilder()
                .UseConventional()
                .UseScanner(new BasicConventionScanner(
                    serviceConventionFake, configurationConventionFake
                ))
                .UseAssemblyCandidateFinder(new DefaultAssemblyCandidateFinder(new[] { typeof(RocketWebHostBuilderTests).Assembly }))
                .UseAssemblyProvider(new DefaultAssemblyProvider(new[] { typeof(RocketWebHostBuilderTests).Assembly }));
            builder.UseStartup<TestStartup>();

            var host = builder.Build();
        }
    }
}
