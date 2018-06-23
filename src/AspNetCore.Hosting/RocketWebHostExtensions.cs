using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.AspNetCore.Hosting.Cli;
using Rocket.Surgery.Extensions.CommandLine;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public static class RocketWebHostExtensions
    {
        public static Task<int> RunCli(this IWebHostBuilder builder)
        {
            return ((IRocketWebHostBuilder)builder).RunCli();
        }

        public static Task<int> RunCliOrStart(this IWebHostBuilder builder)
        {
            return ((IRocketWebHostBuilder)builder).RunCliOrStart();
        }

        public async static Task<int> RunCli(this IRocketWebHostBuilder builder)
        {
            await Task.Yield();

            IWebHost host = null;
            WebHostWrapper webHostWrapper = null;

            builder.ConfigureServices(services =>
                services.AddSingleton(_ => webHostWrapper));
            builder.ConfigureServices(services =>
                services.AddSingleton(_ => host));

            builder.UseServer(new CliServer());
            host = builder.Build();
            webHostWrapper = new WebHostWrapper(host);

            builder.Properties[typeof(IWebHost)] = host;
            builder.Properties[typeof(WebHostWrapper)] = webHostWrapper;

            var executor = host.Services.GetRequiredService<ICommandLineExecutor>();
            
            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<IWebHost>();
            try
            {
                await host.StartAsync();
                var result = executor.Execute(host.Services);
                await host.StopAsync();

                return result;
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Application Crashed");
                return -1;
            }
        }

        public static async Task<int> RunCliOrStart(this IRocketWebHostBuilder builder)
        {
            await Task.Yield();

            IWebHost host = null;

            builder.ConfigureServices(services =>
                services.AddSingleton(_ => new WebHostWrapper(host)));
            builder.ConfigureServices(services =>
                services.AddSingleton(_ => host));

            host = builder.Build();

            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<IWebHost>();
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
                clb.OnParse(state =>
                {
                    applicationState = state;
                    if (!state.IsDefaultCommand)
                    {
                        hostBuilder.UseServer(new CliServer());
                    }
                });
                commandLineAction?.Invoke(clb);
                var executor = clb.Build().Parse(args);

                ((IWebHostBuilder)hostBuilder).ConfigureServices(collection =>
                {
                    collection.AddSingleton(executor);
                    collection.AddSingleton(applicationState);
                });
            });
            return hostBuilder;
        }
    }
}
