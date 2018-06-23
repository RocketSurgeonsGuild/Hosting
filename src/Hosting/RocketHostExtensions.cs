using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.CommandLine;

namespace Rocket.Surgery.Hosting
{
    public static class RocketHostExtensions
    {
        public static Task<int> RunCli(this IHostBuilder builder)
        {
            return ((IRocketHostBuilder)builder).RunCli();
        }

        public static Task<int> RunCliOrStart(this IHostBuilder builder)
        {
            return ((IRocketHostBuilder)builder).RunCliOrStart();
        }

        public static async Task<int> RunCli(this IRocketHostBuilder hostBuilder)
        {
            await Task.Yield();

            var host = hostBuilder.Build();

            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<IHost>();
            try
            {
                await host.StartAsync();
                var result = host.Services.GetRequiredService<ICommandLineExecutor>().Execute(host.Services);
                await host.StopAsync();
                return result;
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Application Crashed");
                return -1;
            }
        }

        public static async Task<int> RunCliOrStart(this IRocketHostBuilder hostBuilder)
        {
            await Task.Yield();

            var host = hostBuilder.Build();

            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<IHost>();
            try
            {
                var executor = host.Services.GetService<ICommandLineExecutor>();
                if (executor != null && !executor.IsDefaultCommand)
                {
                    await host.StartAsync();
                    var result = executor.Execute(host.Services);
                    await host.StopAsync();
                    return result;
                }

                await host.StartAsync();
                await host.WaitForShutdownAsync();
                return 0;
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Application Crashed");
                return -1;
            }
        }

        public static IRocketHostBuilder UseCli(this IRocketHostBuilder hostBuilder, string[] args, Action<ICommandLineBuilder> commandLineAction = null)
        {
            ICommandLineExecutor executor = null;
            IApplicationState applicationState = null;

            hostBuilder.ConfigureAppConfiguration((context, services) =>
            {
                var clb = new CommandLineBuilder(
                    hostBuilder.Scanner,
                    hostBuilder.AssemblyProvider,
                    hostBuilder.AssemblyCandidateFinder,
                    context.Configuration,
                    context.HostingEnvironment,
                    hostBuilder.DiagnosticSource,
                    hostBuilder.Properties
                );
                clb.OnParse(state => { applicationState = state; });
                commandLineAction?.Invoke(clb);
                executor = clb.Build().Parse(args);
            });

            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton(executor);
                services.AddSingleton(applicationState);
            });

            return hostBuilder;
        }
    }
}
