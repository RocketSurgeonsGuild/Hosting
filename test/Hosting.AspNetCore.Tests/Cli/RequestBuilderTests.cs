// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Rocket.Surgery.AspNetCore.Hosting.Cli;
using Xunit;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests.Cli
{
    public class RequestBuilderTests
    {
        [Fact]
        public async Task AddRequestHeader()
        {
            var builder = new WebHostBuilder()
                .Configure(app => { })
                .UseServer(new CliServer());
            var host = builder.Build();
            var server = new WebHostWrapper(host);
            await host.StartAsync();
            server.CreateRequest("/")
                .AddHeader("Host", "MyHost:90")
                .And(request =>
                {
                    Assert.Equal("MyHost:90", request.Headers.Host.ToString());
                });
        }

        [Fact]
        public async Task AddContentHeaders()
        {
            var builder = new WebHostBuilder()
                .Configure(app => { })
                .UseServer(new CliServer());
            var host = builder.Build();
            var server = new WebHostWrapper(host);
            await host.StartAsync();
            server.CreateRequest("/")
                .AddHeader("Content-Type", "Test/Value")
                .And(request =>
                {
                    Assert.NotNull(request.Content);
                    Assert.Equal("Test/Value", request.Content.Headers.ContentType.ToString());
                });
        }
    }
}
