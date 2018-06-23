﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Rocket.Surgery.AspNetCore.Hosting;
using Rocket.Surgery.AspNetCore.Hosting.Cli;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Hosting.AspNetCore.Tests.Startups;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests
{
    public class RocketWebHostStartupTests : AutoTestBase
    {
        public RocketWebHostStartupTests(ITestOutputHelper outputHelper) : base(outputHelper)
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
        public async Task Should_Start_Simple_Application()
        {
            var result = await Builder
                .UseCli(new string[] { }, x => x.OnRun(state => 1337))
                .UseStartup<SimpleStartup>()
                .RunCli();

            result.Should().Be(1337);
        }

        [Fact]
        public async Task Should_Run_Command_Given_Arguments1()
        {
            var result = Builder
                .AppendConvention(new CommandLineConvention())
                .UseCli(new string[] { "dosomething" }, x => x.OnRun(state => 1337));
            Builder.UseStartup<SimpleStartup>();

            (await result.RunCli()).Should().Be(1001);
        }

        [Fact]
        public async Task Should_Start_Application()
        {
            var result = Builder
                .UseCli(new string[] { }, x => x.OnRun(state => 1337));
            Builder.UseStartup<SimpleStartup>();

            using (var server = new TestServer(Builder))
            {
                var response = await server.CreateRequest("/")
                    .GetAsync();

                var content = await response.Content.ReadAsStringAsync();
                content.Should().Be("SimpleStartup -> Configure");
            }
        }

        [Fact]
        public async Task Should_Inject_WebHost_Into_Command()
        {
            var result = Builder
                .UseCli(new[] { "myself" }, x => x.OnRun(state => 1337))
                .AppendDelegate(new CommandLineConventionDelegate(context => context.AddCommand<MyCommand>("myself")))
                .UseStartup<SimpleStartup>();

            (await result.RunCli()).Should().Be(1234);
        }
    }

    [Command]
    class MyCommand
    {
        private readonly IWebHost _webHost;
        private readonly WebHostWrapper _webHostWrapper;

        public MyCommand(IWebHost webHost, WebHostWrapper webHostWrapper)
        {
            _webHost = webHost ?? throw new ArgumentNullException(nameof(webHost));
            _webHostWrapper = webHostWrapper ?? throw new ArgumentNullException(nameof(webHostWrapper));
        }

        public Task<int> OnExecuteAsync()
        {
            return Task.FromResult(1234);
        }
    }
}
