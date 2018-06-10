using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Hosting;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public static class RocketWebHostExtensions
    {
        public static async Task<int> RunCli(this IWebHost host)
        {
            await Task.Yield();
            var executor = host.Services.GetRequiredService<ICommandLineExecutor>();
            return executor.Execute(host.Services);
        }

        public static async Task<int> RunCliOrStart(this IWebHost host)
        {
            await Task.Yield();
            var executor = host.Services.GetService<ICommandLineExecutor>();

            if (executor != null && !executor.IsDefaultCommand)
            {
                return executor.Execute(host.Services);
            }

            await host.StartAsync();
            await host.WaitForShutdownAsync();
            return 0;
        }
        public static async Task<int> RunCli(this IWebHostBuilder builder)
        {
            await Task.Yield();
            var host = builder.Build();
            var executor = host.Services.GetRequiredService<ICommandLineExecutor>();
            return executor.Execute(host.Services);
        }

        public static async Task<int> RunCliOrStart(this IWebHostBuilder builder)
        {
            await Task.Yield();
            var host = builder.Build();
            var executor = host.Services.GetService<ICommandLineExecutor>();

            if (executor != null && !executor.IsDefaultCommand)
            {
                return executor.Execute(host.Services);
            }

            await host.StartAsync();
            await host.WaitForShutdownAsync();
            return 0;
        }

        public static async Task<int> RunCliOrStart(this IRocketWebHostBuilder builder, string[] args)
        {
            await Task.Yield();
            builder
                .UseCli(args, commandLineBuilder =>
                {
                });
            var host = builder.Build();
            var executor = host.Services.GetService<ICommandLineExecutor>();
            if (executor != null && !executor.IsDefaultCommand)
            {
                return executor.Execute(host.Services);
            }

            await host.StartAsync();
            await host.WaitForShutdownAsync();
            return 0;
        }

        public static IRocketWebHostBuilder UseCli(this IRocketWebHostBuilder hostBuilder, string[] args, Action<ICommandLineBuilder> commandLineAction = null)
        {
            hostBuilder.ConfigureAppConfiguration((context, services) =>
            {
                IApplicationState applicationState = null;
                var clb = new CommandLineBuilder(
                    hostBuilder.Scanner,
                    hostBuilder.AssemblyProvider,
                    hostBuilder.AssemblyCandidateFinder,
                    context.Configuration,
                    (IHostingEnvironment)context.HostingEnvironment,
                    hostBuilder.DiagnosticSource,
                    hostBuilder.Properties
                );
                clb.OnParse(state => { applicationState = state; });
                commandLineAction?.Invoke(clb);
                var executor = clb.Build().Parse(args);

                hostBuilder.ConfigureServices(collection =>
                {
                    collection.AddSingleton(executor);
                    collection.AddSingleton(applicationState);
                });
            });
            return hostBuilder;
        }
    }
}
