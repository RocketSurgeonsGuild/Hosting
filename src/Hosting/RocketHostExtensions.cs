using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Hosting
{
    /// <summary>
    /// Class RocketHostExtensions.
    /// </summary>
    public static class RocketHostExtensions
    {
        private static readonly ConditionalWeakTable<IHostBuilder, RocketHostBuilder> Builders = new ConditionalWeakTable<IHostBuilder, RocketHostBuilder>();

        /// <summary>
        /// Configures the rocket Surgery.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="action">The action.</param>
        /// <returns>IHostBuilder.</returns>
        public static IHostBuilder ConfigureRocketSurgery(this IHostBuilder builder, Action<IRocketHostBuilder> action)
        {
            action(GetOrCreateBuilder(builder));
            return builder;
        }

        /// <summary>
        /// Uses the rocket booster.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="func">The function.</param>
        /// <param name="action">The action.</param>
        /// <returns>IHostBuilder.</returns>
        public static IHostBuilder UseRocketBooster(this IHostBuilder builder, Func<IHostBuilder, IRocketHostBuilder> func, Action<IRocketHostBuilder> action = null)
        {
            var b = func(builder);
            action?.Invoke(b);
            return builder;
        }

        /// <summary>
        /// Launches the with.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="func">The function.</param>
        /// <param name="action">The action.</param>
        /// <returns>IHostBuilder.</returns>
        public static IHostBuilder LaunchWith(this IHostBuilder builder, Func<IHostBuilder, IRocketHostBuilder> func, Action<IRocketHostBuilder> action = null)
        {
            var b = func(builder);
            action?.Invoke(b);
            return builder;
        }

        /// <summary>
        /// Uses the scanner.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="scanner">The scanner.</param>
        /// <returns>IRocketHostBuilder.</returns>
        public static IRocketHostBuilder UseScanner(this IRocketHostBuilder builder, IConventionScanner scanner)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(scanner));
        }

        /// <summary>
        /// Uses the assembly candidate finder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assemblyCandidateFinder">The assembly candidate finder.</param>
        /// <returns>IRocketHostBuilder.</returns>
        public static IRocketHostBuilder UseAssemblyCandidateFinder(this IRocketHostBuilder builder, IAssemblyCandidateFinder assemblyCandidateFinder)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(assemblyCandidateFinder));
        }

        /// <summary>
        /// Uses the assembly provider.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assemblyProvider">The assembly provider.</param>
        /// <returns>IRocketHostBuilder.</returns>
        public static IRocketHostBuilder UseAssemblyProvider(this IRocketHostBuilder builder, IAssemblyProvider assemblyProvider)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(assemblyProvider));
        }

        /// <summary>
        /// Uses the diagnostic source.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="diagnosticSource">The diagnostic source.</param>
        /// <returns>IRocketHostBuilder.</returns>
        public static IRocketHostBuilder UseDiagnosticSource(this IRocketHostBuilder builder, DiagnosticSource diagnosticSource)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(diagnosticSource));
        }

        /// <summary>
        /// Uses the dependency context.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="dependencyContext">The dependency context.</param>
        /// <param name="diagnosticSource">The diagnostic source.</param>
        /// <returns>IRocketHostBuilder.</returns>
        public static IRocketHostBuilder UseDependencyContext(
            this IRocketHostBuilder builder,
            DependencyContext dependencyContext,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForDependencyContext(dependencyContext, diagnosticSource)(builder.Builder);
        }

        /// <summary>
        /// Uses the application domain.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="appDomain">The application domain.</param>
        /// <param name="diagnosticSource">The diagnostic source.</param>
        /// <returns>IRocketHostBuilder.</returns>
        public static IRocketHostBuilder UseAppDomain(
            this IRocketHostBuilder builder,
            AppDomain appDomain,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForAppDomain(appDomain, diagnosticSource)(builder.Builder);
        }

        /// <summary>
        /// Uses the assemblies.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="diagnosticSource">The diagnostic source.</param>
        /// <returns>IRocketHostBuilder.</returns>
        public static IRocketHostBuilder UseAssemblies(
            this IRocketHostBuilder builder,
            IEnumerable<Assembly> assemblies,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForAssemblies(assemblies, diagnosticSource)(builder.Builder);
        }

        /// <summary>
        /// Uses the diagnostic logging.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="action">The action.</param>
        /// <returns>IRocketHostBuilder.</returns>
        public static IRocketHostBuilder UseDiagnosticLogging(this IRocketHostBuilder builder, Action<ILoggingBuilder> action)
        {
            DiagnosticListenerExtensions.SubscribeWithAdapter(
                builder.DiagnosticSource is DiagnosticListener listener ? listener : new DiagnosticListener("DiagnosticLogger"),
                new DiagnosticListenerLoggingAdapter(
                    new ServiceCollection()
                        .AddLogging(action)
                        .BuildServiceProvider()
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("DiagnosticLogger")
                    )
                );
            return builder;
        }

        /// <summary>
        /// Gets the conventional host builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>RocketHostBuilder.</returns>
        internal static RocketHostBuilder GetConventionalHostBuilder(IHostBuilder builder)
        {
            return GetOrCreateBuilder(builder);
        }

        /// <summary>
        /// Gets the or create builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>RocketHostBuilder.</returns>
        internal static RocketHostBuilder GetOrCreateBuilder(IRocketHostBuilder builder)
        {
            return GetOrCreateBuilder(builder.Builder);
        }

        /// <summary>
        /// Gets the or create builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>RocketHostBuilder.</returns>
        internal static RocketHostBuilder GetOrCreateBuilder(IHostBuilder builder)
        {
            if (!Builders.TryGetValue(builder, out var conventionalBuilder))
            {
                var diagnosticSource = new DiagnosticListener("Rocket.Surgery.Hosting");
                var dependencyContext = DependencyContext.Default;
                var logger = new DiagnosticLogger(diagnosticSource);
                var serviceProviderDictionary = new ServiceProviderDictionary(builder.Properties);
                serviceProviderDictionary.Set<ILogger>(logger);
                var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(dependencyContext, logger);
                var assemblyProvider = new DependencyContextAssemblyProvider(dependencyContext, logger);
                var scanner = new SimpleConventionScanner(assemblyCandidateFinder, serviceProviderDictionary, logger);
                conventionalBuilder = new RocketHostBuilder(builder, scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, serviceProviderDictionary);

                var host = new RocketContext(builder);
                builder
                    .ConfigureHostConfiguration(host.CaptureArguments)
                    .ConfigureHostConfiguration(host.ConfigureCli)
                    .ConfigureAppConfiguration(host.ReplaceArguments)
                    .ConfigureAppConfiguration(host.ConfigureAppConfiguration)
                    .ConfigureServices(host.ConfigureServices)
                    .ConfigureServices(host.DefaultServices);
                Builders.Add(builder, conventionalBuilder);
            }

            return conventionalBuilder;
        }

        /// <summary>
        /// Swaps the specified builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="newRocketBuilder">The new rocket builder.</param>
        /// <returns>RocketHostBuilder.</returns>
        internal static RocketHostBuilder Swap(IRocketHostBuilder builder, RocketHostBuilder newRocketBuilder)
        {
            Builders.Remove(builder.Builder);
            Builders.Add(builder.Builder, newRocketBuilder);
            return newRocketBuilder;
        }

        /// <summary>
        /// Uses the command line.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>IRocketHostBuilder.</returns>
        public static IRocketHostBuilder UseCommandLine(this IRocketHostBuilder builder)
        {
            return builder.UseCommandLine(x => x.SuppressStatusMessages = true);
        }

        /// <summary>
        /// Uses the command line.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="configureOptions">The configure options.</param>
        /// <returns>IRocketHostBuilder.</returns>
        public static IRocketHostBuilder UseCommandLine(this IRocketHostBuilder builder, Action<ConsoleLifetimeOptions> configureOptions)
        {
            builder.Builder.Properties[typeof(CommandLineHostedService)] = true;
            builder.Builder
                .UseConsoleLifetime()
                .ConfigureServices(services => services.Configure(configureOptions));
            return GetOrCreateBuilder(builder);
        }

        /// <summary>
        /// Runs the cli.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>Task&lt;System.Int32&gt;.</returns>
        public static async Task<int> RunCli(this IHostBuilder builder)
        {
            builder.ConfigureRocketSurgery(x => x.UseCommandLine());
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
