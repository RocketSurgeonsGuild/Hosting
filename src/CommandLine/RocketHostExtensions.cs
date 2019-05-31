using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using NetEscapades.Configuration.Yaml;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Hosting;
using YamlDotNet.Core;
using ConfigurationBuilder = Rocket.Surgery.Extensions.Configuration.ConfigurationBuilder;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting
{
    public static class RocketCommandLineHostExtensions
    {
        public static IRocketHostBuilder UseCommandLine(this IRocketHostBuilder builder)
        {
            builder.Properties.Add(nameof(UseCommandLine), true);
            builder.Builder
                .UseConsoleLifetime()
                .ConfigureServices(services => services.Configure<ConsoleLifetimeOptions>(c => c.SuppressStatusMessages = true));
            return RocketHostExtensions.GetOrCreateBuilder(builder);
        }

        public static async Task<int> RunCli(this IHostBuilder builder)
        {
            builder.ConfigureRocketSurgey(x => x.UseCommandLine());
            using (var host = builder.Build())
            {
                try
                {
                    await host.StartAsync();
                    var context = host.Services.GetRequiredService<ICommandLineExecutor>();
                    var result = context.Execute(host.Services);
                    await host.StopAsync();
                    return result;
                }
                catch (Exception e)
                {
                    host.Services.GetService<ILoggerFactory>()
                        .CreateLogger("Cli")
                        .LogError(e, "Application exception");
                    return -1;
                }
            }
        }
    }
}

