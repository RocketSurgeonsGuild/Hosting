using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Logging;

namespace Rocket.Surgery.Hosting
{
    public static class RocketHostExtensions
    {
        public static Task<int> GoAsync(this IHostBuilder builder)
        {
            return ((IRocketHostBuilder)builder).GoAsync(CancellationToken.None);
        }

        public static Task<int> GoAsync(this IHostBuilder builder, CancellationToken cancellationToken)
        {
            return ((IRocketHostBuilder)builder).GoAsync(cancellationToken);
        }

        public static int Go(this IHostBuilder builder)
        {
            return ((IRocketHostBuilder)builder).GoAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public static async Task<int> GoAsync(this IRocketHostBuilder hostBuilder)
        {
            return hostBuilder.GoAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public static async Task<int> GoAsync(this IRocketHostBuilder hostBuilder, CancellationToken cancellationToken)
        {
            await Task.Yield();
            using (var host = hostBuilder.Build())
            {
                var lifetime = host.Services.GetRequiredService<IHostLifetime>() as CliLifetime;
                await host.RunAsync(cancellationToken);
                return lifetime?.Result ?? 0;
            }
        }

        public static T ContributeCommandLine<T>(this T builder, CommandLineConventionDelegate commandLineConventionDelegate)
            where T : IRocketHostBuilder
        {
            builder.AppendDelegate(commandLineConventionDelegate);
            return builder;
        }

        public static T ContributeConfiguration<T>(this T builder, ConfigurationConventionDelegate configurationConventionDelegate)
            where T : IRocketHostBuilder
        {
            builder.AppendDelegate(configurationConventionDelegate);
            return builder;
        }

        public static T ContributeLogging<T>(this T builder, LoggingConventionDelegate loggingConventionDelegate)
            where T : IRocketHostBuilder
        {
            builder.AppendDelegate(loggingConventionDelegate);
            return builder;
        }

        public static T ContributeServices<T>(this T builder, ServiceConventionDelegate serviceConventionDelegate)
            where T : IRocketHostBuilder
        {
            builder.AppendDelegate(serviceConventionDelegate);
            return builder;
        }
    }
}
