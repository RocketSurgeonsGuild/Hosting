// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.AspNetCore.Hosting.Cli;
using Xunit;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests.Cli
{
    public class HttpContextBuilderTests
    {
        [Fact]
        public async Task ExpectedValuesAreAvailable()
        {
            var server = new CliServer();
            var builder = new WebHostBuilder()
                .Configure(app => { })
                .UseServer(server);
            var host = builder.Build();
            await host.StartAsync();
            server.BaseAddress = new Uri("https://example.com/A/Path/");
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Post;
                c.Request.Path = "/and/file.txt";
                c.Request.QueryString = new QueryString("?and=query");
            });

            Assert.True(context.RequestAborted.CanBeCanceled);
            Assert.Equal("HTTP/1.1", context.Request.Protocol);
            Assert.Equal("POST", context.Request.Method);
            Assert.Equal("https", context.Request.Scheme);
            Assert.Equal("example.com", context.Request.Host.Value);
            Assert.Equal("/A/Path", context.Request.PathBase.Value);
            Assert.Equal("/and/file.txt", context.Request.Path.Value);
            Assert.Equal("?and=query", context.Request.QueryString.Value);
            Assert.NotNull(context.Request.Body);
            Assert.NotNull(context.Request.Headers);
            Assert.NotNull(context.Response.Headers);
            Assert.NotNull(context.Response.Body);
            Assert.Equal(404, context.Response.StatusCode);
            Assert.Null(context.Features.Get<IHttpResponseFeature>().ReasonPhrase);
        }

        [Fact]
        public async Task SingleSlashNotMovedToPathBase()
        {
            var server = new CliServer();
            var builder = new WebHostBuilder()
                .Configure(app => { })
                .UseServer(server);
            var host = builder.Build();
            await host.StartAsync();
            var context = await server.SendAsync(c =>
            {
                c.Request.Path = "/";
            });

            Assert.Equal("", context.Request.PathBase.Value);
            Assert.Equal("/", context.Request.Path.Value);
        }

        [Fact]
        public async Task MiddlewareOnlySetsHeaders()
        {
            var server = new CliServer();
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Run(c =>
                    {
                        c.Response.Headers["TestHeader"] = "TestValue";
                        return Task.FromResult(0);
                    });
                })
                .UseServer(server);
            var host = builder.Build();
            await host.StartAsync();
            var context = await server.SendAsync(c => { });

            Assert.Equal("TestValue", context.Response.Headers["TestHeader"]);
        }

        [Fact]
        public async Task BlockingMiddlewareShouldNotBlockClient()
        {
            var block = new ManualResetEvent(false);
            var server = new CliServer();
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Run(c =>
                    {
                        block.WaitOne();
                        return Task.FromResult(0);
                    });
                })
                .UseServer(server);
            var host = builder.Build();
            await host.StartAsync();
            var task = server.SendAsync(c => { });

            Assert.False(task.IsCompleted);
            Assert.False(task.Wait(50));
            block.Set();
            var context = await task;
        }

        [Fact]
        public async Task HeadersAvailableBeforeSyncBodyFinished()
        {
            var block = new ManualResetEvent(false);
            var server = new CliServer();
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Run(c =>
                    {
                        c.Response.Headers["TestHeader"] = "TestValue";
                        var bytes = System.Text.Encoding.UTF8.GetBytes("BodyStarted" + Environment.NewLine);
                        c.Response.Body.Write(bytes, 0, bytes.Length);
                        Assert.True(block.WaitOne(TimeSpan.FromSeconds(5)));
                        bytes = System.Text.Encoding.UTF8.GetBytes("BodyFinished");
                        c.Response.Body.Write(bytes, 0, bytes.Length);
                        return Task.CompletedTask;
                    });
                })
                .UseServer(server);
            var host = builder.Build();
            await host.StartAsync();
            var context = await server.SendAsync(c => { });

            Assert.Equal("TestValue", context.Response.Headers["TestHeader"]);
            var reader = new StreamReader(context.Response.Body);
            Assert.Equal("BodyStarted", reader.ReadLine());
            block.Set();
            Assert.Equal("BodyFinished", reader.ReadToEnd());
        }

        [Fact]
        public async Task HeadersAvailableBeforeAsyncBodyFinished()
        {
            var block = new ManualResetEvent(false);
            var server = new CliServer();
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Run(async c =>
                    {
                        c.Response.Headers["TestHeader"] = "TestValue";
                        await c.Response.WriteAsync("BodyStarted" + Environment.NewLine);
                        Assert.True(block.WaitOne(TimeSpan.FromSeconds(5)));
                        await c.Response.WriteAsync("BodyFinished");
                    });
                })
                .UseServer(server);
            var host = builder.Build();
            await host.StartAsync();
            var context = await server.SendAsync(c => { });

            Assert.Equal("TestValue", context.Response.Headers["TestHeader"]);
            var reader = new StreamReader(context.Response.Body);
            Assert.Equal("BodyStarted", await reader.ReadLineAsync());
            block.Set();
            Assert.Equal("BodyFinished", await reader.ReadToEndAsync());
        }

        [Fact]
        public async Task FlushSendsHeaders()
        {
            var block = new ManualResetEvent(false);
            var server = new CliServer();
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Run(async c =>
                    {
                        c.Response.Headers["TestHeader"] = "TestValue";
                        c.Response.Body.Flush();
                        block.WaitOne();
                        await c.Response.WriteAsync("BodyFinished");
                    });
                })
                .UseServer(server);
            var host = builder.Build();
            await host.StartAsync();
            var context = await server.SendAsync(c => { });

            Assert.Equal("TestValue", context.Response.Headers["TestHeader"]);
            block.Set();
            Assert.Equal("BodyFinished", new StreamReader(context.Response.Body).ReadToEnd());
        }

        [Fact]
        public async Task ClientDisposalCloses()
        {
            var block = new ManualResetEvent(false);
            var server = new CliServer();
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Run(async c =>
                    {
                        c.Response.Headers["TestHeader"] = "TestValue";
                        c.Response.Body.Flush();
                        block.WaitOne();
                        await c.Response.WriteAsync("BodyFinished");
                    });
                })
                .UseServer(server);
            var host = builder.Build();
            await host.StartAsync();
            var context = await server.SendAsync(c => { });

            Assert.Equal("TestValue", context.Response.Headers["TestHeader"]);
            var responseStream = context.Response.Body;
            Task<int> readTask = responseStream.ReadAsync(new byte[100], 0, 100);
            Assert.False(readTask.IsCompleted);
            responseStream.Dispose();
            Assert.True(readTask.Wait(TimeSpan.FromSeconds(10)));
            Assert.Equal(0, readTask.Result);
            block.Set();
        }

        [Fact]
        public async Task ClientCancellationAborts()
        {
            var block = new ManualResetEvent(false);
            var server = new CliServer();
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Run(c =>
                    {
                        block.Set();
                        Assert.True(c.RequestAborted.WaitHandle.WaitOne(TimeSpan.FromSeconds(10)));
                        c.RequestAborted.ThrowIfCancellationRequested();
                        return Task.CompletedTask;
                    });
                })
                .UseServer(server);
            var host = builder.Build();
            await host.StartAsync();
            var cts = new CancellationTokenSource();
            var contextTask = server.SendAsync(c => { }, cts.Token);
            block.WaitOne();
            cts.Cancel();

            var ex = Assert.Throws<AggregateException>(() => contextTask.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsAssignableFrom<OperationCanceledException>(ex.GetBaseException());
        }

        [Fact]
        public async Task ClientCancellationAbortsReadAsync()
        {
            var block = new ManualResetEvent(false);
            var server = new CliServer();
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Run(async c =>
                    {
                        c.Response.Headers["TestHeader"] = "TestValue";
                        c.Response.Body.Flush();
                        block.WaitOne();
                        await c.Response.WriteAsync("BodyFinished");
                    });
                })
                .UseServer(server);
            var host = builder.Build();
            await host.StartAsync();
            var context = await server.SendAsync(c => { });

            Assert.Equal("TestValue", context.Response.Headers["TestHeader"]);
            var responseStream = context.Response.Body;
            var cts = new CancellationTokenSource();
            var readTask = responseStream.ReadAsync(new byte[100], 0, 100, cts.Token);
            Assert.False(readTask.IsCompleted);
            cts.Cancel();
            var ex = Assert.Throws<AggregateException>(() => readTask.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsAssignableFrom<OperationCanceledException>(ex.GetBaseException());
            block.Set();
        }

        [Fact]
        public async Task ExceptionBeforeFirstWriteIsReported()
        {
            var server = new CliServer();
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Run(c =>
                    {
                        throw new InvalidOperationException("Test Exception");
                    });
                })
                .UseServer(server);
            var host = builder.Build();
            await host.StartAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => server.SendAsync(c => { }));
        }

        [Fact]
        public async Task ExceptionAfterFirstWriteIsReported()
        {
            var block = new ManualResetEvent(false);
            var server = new CliServer();
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Run(async c =>
                    {
                        c.Response.Headers["TestHeader"] = "TestValue";
                        await c.Response.WriteAsync("BodyStarted");
                        block.WaitOne();
                        throw new InvalidOperationException("Test Exception");
                    });
                })
                .UseServer(server);
            var host = builder.Build();
            await host.StartAsync();
            var context = await server.SendAsync(c => { });

            Assert.Equal("TestValue", context.Response.Headers["TestHeader"]);
            Assert.Equal(11, context.Response.Body.Read(new byte[100], 0, 100));
            block.Set();
            var ex = Assert.Throws<IOException>(() => context.Response.Body.Read(new byte[100], 0, 100));
            Assert.IsAssignableFrom<InvalidOperationException>(ex.InnerException);
        }

        [Fact]
        public async Task ClientHandlerCreateContextWithDefaultRequestParameters()
        {
            // This logger will attempt to access information from HttpRequest once the HttpContext is created
            var logger = new VerifierLogger();
            var server = new CliServer();
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ILogger<IWebHost>>(logger);
                })
                .Configure(app =>
                {
                    app.Run(context =>
                    {
                        return Task.FromResult(0);
                    });
                })
                .UseServer(server);
            var host = builder.Build();
            await host.StartAsync();

            // The HttpContext will be created and the logger will make sure that the HttpRequest exists and contains reasonable values
            var ctx = await server.SendAsync(c => { });
        }

        private class VerifierLogger : ILogger<IWebHost>
        {
            public IDisposable BeginScope<TState>(TState state) => new NoopDispoasble();

            public bool IsEnabled(LogLevel logLevel) => true;

            // This call verifies that fields of HttpRequest are accessed and valid
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) => formatter(state, exception);

            class NoopDispoasble : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}
