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
using Rocket.Surgery.CommandLine;
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
        public static IRocketHostBuilder UseCommandLine(this IHostBuilder builder)
        {
            if (!builder.Properties.TryGetValue(typeof(CommandLineHost), out var value))
            {
                value = new CommandLineHost(builder);
                builder.Properties.Add(typeof(CommandLineHost), value);

                var host = (CommandLineHost)value;
                builder.UseConsoleLifetime()
                    .ConfigureHostConfiguration(host.CaptureAndRemoveArguments)
                    .ConfigureAppConfiguration(host.CaptureAndRemoveArguments)
                    .ConfigureServices(host.ConfigureServices);
            }
            return RocketHostExtensions.GetOrCreateBuilder(builder);
        }

        public static async Task<int> RunCli(this IHostBuilder builder)
        {
            builder.UseCommandLine();
            var host = builder.Build();
            try
            {
                await host.StartAsync();
                var context = host.Services.GetRequiredService<IRocketHostingContext>();
                var clb = new CommandLineBuilder(
                    context.Scanner,
                    context.AssemblyProvider,
                    context.AssemblyCandidateFinder,
                    context.DiagnosticSource,
                    context.Properties
                );
                return clb.Build().Execute(host.Services, context.Arguments);
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
