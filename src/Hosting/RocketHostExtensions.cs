using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting
{
    public static class RocketHostExtensions
    {
        private static readonly ConditionalWeakTable<IHostBuilder, RocketHostBuilder> Builders = new ConditionalWeakTable<IHostBuilder, RocketHostBuilder>();

        public static IHostBuilder ConfigureRocketSurgey(this IHostBuilder builder, Action<IRocketHostBuilder> action)
        {
            action(GetOrCreateBuilder(builder));
            return builder;
        }

        public static IHostBuilder UseRocketBooster(this IHostBuilder builder, Func<IHostBuilder, IRocketHostBuilder> func, Action<IRocketHostBuilder> action = null)
        {
            var b = func(builder);
            action?.Invoke(b);
            return builder;
        }

        public static IHostBuilder LaunchWith(this IHostBuilder builder, Func<IHostBuilder, IRocketHostBuilder> func, Action<IRocketHostBuilder> action = null)
        {
            var b = func(builder);
            action?.Invoke(b);
            return builder;
        }

        public static IRocketHostBuilder UseScanner(this IRocketHostBuilder builder, IConventionScanner scanner)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(scanner));
        }

        public static IRocketHostBuilder UseAssemblyCandidateFinder(this IRocketHostBuilder builder, IAssemblyCandidateFinder assemblyCandidateFinder)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(assemblyCandidateFinder));
        }

        public static IRocketHostBuilder UseAssemblyProvider(this IRocketHostBuilder builder, IAssemblyProvider assemblyProvider)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(assemblyProvider));
        }

        public static IRocketHostBuilder UseDiagnosticSource(this IRocketHostBuilder builder, DiagnosticSource diagnosticSource)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(diagnosticSource));
        }

        public static IRocketHostBuilder UseDependencyContext(
            this IRocketHostBuilder builder,
            DependencyContext dependencyContext,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForDependencyContext(dependencyContext, diagnosticSource)(builder.Builder);
        }

        public static IRocketHostBuilder UseAppDomain(
            this IRocketHostBuilder builder,
            AppDomain appDomain,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForAppDomain(appDomain, diagnosticSource)(builder.Builder);
        }

        public static IRocketHostBuilder UseAssemblies(
            this IRocketHostBuilder builder,
            IEnumerable<Assembly> assemblies,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForAssemblies(assemblies, diagnosticSource)(builder.Builder);
        }

        internal static RocketHostBuilder GetConventionalHostBuilder(IHostBuilder builder)
        {
            return GetOrCreateBuilder(builder);
        }

        internal static RocketHostBuilder GetOrCreateBuilder(IRocketHostBuilder builder)
        {
            return GetOrCreateBuilder(builder.Builder);
        }

        internal static RocketHostBuilder GetOrCreateBuilder(IHostBuilder builder)
        {
            if (!Builders.TryGetValue(builder, out var conventionalBuilder))
            {
                var diagnosticSource = new DiagnosticListener("Rocket.Surgery.Hosting");
                var dependencyContext = DependencyContext.Default;
                var logger = new DiagnosticLogger(diagnosticSource);
                var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(dependencyContext, logger);
                var assemblyProvider = new DependencyContextAssemblyProvider(dependencyContext, logger);
                var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
                conventionalBuilder = new RocketHostBuilder(builder, scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, builder.Properties);

                var host = new RocketContext(builder);
                builder
                    .ConfigureHostConfiguration(host.CaptureArguments)
                    .ConfigureHostConfiguration(host.ConfigureCli)
                    .ConfigureAppConfiguration(host.ReplaceArguments)
                    .ConfigureAppConfiguration(host.ConfigureAppConfiguration)
                    .ConfigureServices(host.ConfigureServices)
                    .ConfigureServices((context, services) => host.DefaultServices(builder, context, services));
                Builders.Add(builder, conventionalBuilder);
            }

            return conventionalBuilder;
        }

        internal static RocketHostBuilder Swap(IRocketHostBuilder builder, RocketHostBuilder newRocketBuilder)
        {
            Builders.Remove(builder.Builder);
            Builders.Add(builder.Builder, newRocketBuilder);
            return newRocketBuilder;
        }
        public static IRocketHostBuilder UseCommandLine(this IRocketHostBuilder builder)
        {
            return builder.UseCommandLine(x => x.SuppressStatusMessages = true);
        }
        public static IRocketHostBuilder UseCommandLine(this IRocketHostBuilder builder, Action<ConsoleLifetimeOptions> configureOptions)
        {
            builder.Properties.Add(nameof(UseCommandLine), true);
            builder.Builder
                .UseConsoleLifetime()
                .ConfigureServices(services => services.Configure<ConsoleLifetimeOptions>(configureOptions));
            return RocketHostExtensions.GetOrCreateBuilder(builder);
        }

        public static async Task<int> RunCli(this IHostBuilder builder)
        {
            builder.ConfigureRocketSurgey(x => x.UseCommandLine());
            using (var host = builder.Build())
            {
                var result = host.Services.GetRequiredService<CommandLineResult>();
                try
                {
                    await host.RunAsync();
                    return result.Value;
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
