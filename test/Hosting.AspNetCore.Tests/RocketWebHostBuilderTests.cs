﻿using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using McMaster.Extensions.CommandLineUtils;
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
        public void Builder_Should_Be_Castable_ToHostBuilder()
        {
            AutoFake.Provide(WebHost.CreateDefaultBuilder());
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            builder.AsWebHostBuilder().Should().BeAssignableTo<IWebHostBuilder>();
        }

        [Fact]
        public void Should_Call_Through_To_Delegate_Methods()
        {
            AutoFake.Provide(WebHost.CreateDefaultBuilder());
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            builder.PrependDelegate(new Action(() => { }));
            builder.AppendDelegate(new Action(() => { }));
            builder.ConfigureServices((context, collection) => { });
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().PrependDelegate(A<Delegate>._)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => AutoFake.Resolve<IConventionScanner>().AppendDelegate(A<Delegate>._)).MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public void Should_Call_Through_To_Convention_Methods()
        {
            AutoFake.Provide(WebHost.CreateDefaultBuilder());
            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
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
            AutoFake.Provide(WebHost.CreateDefaultBuilder());
            AutoFake.Provide<IConventionScanner>(new BasicConventionScanner(
                serviceConventionFake, configurationConventionFake, commandLineConventionFake
            ));
            AutoFake.Provide<IAssemblyCandidateFinder>(
                new DefaultAssemblyCandidateFinder(new[] { typeof(RocketWebHostBuilderTests).Assembly }));
            AutoFake.Provide<IAssemblyProvider>(
                new DefaultAssemblyProvider(new[] { typeof(RocketWebHostBuilderTests).Assembly }));

            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();
            builder.UseStartup<TestStartup>();

            var host = builder.Build();
        }

        [Fact]
        public void Should_Run_Rocket_CommandLine()
        {
            AutoFake.Provide(WebHost.CreateDefaultBuilder());
            AutoFake.Provide<IConventionScanner>(new BasicConventionScanner());
            AutoFake.Provide<IAssemblyCandidateFinder>(
                new DefaultAssemblyCandidateFinder(new[] { typeof(RocketWebHostBuilderTests).Assembly }));
            AutoFake.Provide<IAssemblyProvider>(
                new DefaultAssemblyProvider(new[] { typeof(RocketWebHostBuilderTests).Assembly }));

            IRocketWebHostBuilder builder = AutoFake.Resolve<RocketWebHostBuilder>();

            var result = builder
                .UseCli(new string[] { }, x => x.OnRun(state => 1337));
            builder.UseStartup<TestStartup>();

            var host = result.Build();
            host.Services.GetService<ICommandLineExecutor>().Execute(host.Services)
                .Should().Be(1337);
        }
    }
}
