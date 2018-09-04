using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.AspNetCore.Hosting.Cli;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Logging;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public static class RocketWebHostExtensions
    {
        public static Task<int> GoAsync(this IWebHostBuilder builder)
        {
            return ((IRocketWebHostBuilder)builder).GoAsync(CancellationToken.None);
        }

        public static Task<int> GoAsync(this IWebHostBuilder builder, CancellationToken cancellationToken)
        {
            return ((IRocketWebHostBuilder)builder).GoAsync(cancellationToken);
        }

        public static int Go(this IWebHostBuilder builder)
        {
            return ((IRocketWebHostBuilder)builder).GoAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public static Task<int> GoAsync(this IRocketWebHostBuilder builder)
        {
            return builder.GoAsync(CancellationToken.None);
        }

        public static int Go(this IRocketWebHostBuilder builder)
        {
            return builder.GoAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public static async Task<int> GoAsync(this IRocketWebHostBuilder hostBuilder, CancellationToken cancellationToken)
        {
            await Task.Yield();

            IWebHost host = null;

            hostBuilder.ConfigureServices(services =>
                services.AddSingleton(_ => new WebHostWrapper(host)));
            hostBuilder.ConfigureServices(services =>
                services.AddSingleton(_ => host));

            using (host = hostBuilder.Build())
            {
                var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<IWebHost>();
                try
                {
                    var executor = host.Services.GetService<ICommandLineExecutor>();
                    if (executor != null)
                    {
                        if (executor.Application.IsShowingInformation)
                        {
                            return 0;
                        }

                        if (!executor.IsDefaultCommand)
                        {
                            await host.StartAsync(cancellationToken);
                            var result = executor.Execute(host.Services);
                            await host.StopAsync(cancellationToken);
                            return result;
                        }
                    }

                    var r = executor.Execute(host.Services);
                    if (r == int.MinValue)
                    {
                        await host.RunAsync(cancellationToken);
                    }
                    else
                    {
                        return r;
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    logger.LogCritical(e, "Application Crashed");
                    return -1;
                }
            }
        }

        public static T ContributeCommandLine<T>(this T builder, CommandLineConventionDelegate commandLineConventionDelegate)
            where T : IRocketWebHostBuilder
        {
            builder.AppendDelegate(commandLineConventionDelegate);
            return builder;
        }

        public static T ContributeConfiguration<T>(this T builder, ConfigurationConventionDelegate configurationConventionDelegate)
            where T : IRocketWebHostBuilder
        {
            builder.AppendDelegate(configurationConventionDelegate);
            return builder;
        }

        public static T ContributeLogging<T>(this T builder, LoggingConventionDelegate loggingConventionDelegate)
            where T : IRocketWebHostBuilder
        {
            builder.AppendDelegate(loggingConventionDelegate);
            return builder;
        }

        public static T ContributeServices<T>(this T builder, ServiceConventionDelegate serviceConventionDelegate)
            where T : IRocketWebHostBuilder
        {
            builder.AppendDelegate(serviceConventionDelegate);
            return builder;
        }
    }
}
