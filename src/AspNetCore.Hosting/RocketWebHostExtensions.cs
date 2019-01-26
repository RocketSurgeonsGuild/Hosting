using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.AspNetCore.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Hosting
{
    public static class RocketWebHostExtensions
    {
        private static readonly ConditionalWeakTable<IWebHostBuilder, RocketWebHostBuilder> Builders = new ConditionalWeakTable<IWebHostBuilder, RocketWebHostBuilder>();

        public static IRocketWebHostBuilder UseRocketBooster(this IWebHostBuilder builder, Func<IWebHostBuilder, IRocketWebHostBuilder> func)
        {
            return func(builder);
        }

        public static IRocketWebHostBuilder LaunchWith(this IWebHostBuilder builder, Func<IWebHostBuilder, IRocketWebHostBuilder> func)
        {
            return func(builder);
        }

        public static IRocketWebHostBuilder UseConventional(this IWebHostBuilder builder)
        {
            return GetOrCreateBuilder(builder);
        }

        public static IRocketWebHostBuilder UseScanner(this IRocketWebHostBuilder builder, IConventionScanner scanner)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(scanner));
        }

        public static IRocketWebHostBuilder UseAssemblyCandidateFinder(this IRocketWebHostBuilder builder, IAssemblyCandidateFinder assemblyCandidateFinder)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(assemblyCandidateFinder));
        }

        public static IRocketWebHostBuilder UseAssemblyProvider(this IRocketWebHostBuilder builder, IAssemblyProvider assemblyProvider)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(assemblyProvider));
        }

        public static IRocketWebHostBuilder UseDiagnosticSource(this IRocketWebHostBuilder builder, DiagnosticSource diagnosticSource)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(diagnosticSource));
        }

        public static IRocketWebHostBuilder UseProperties(this IRocketWebHostBuilder builder, IDictionary<object, object> properties)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(properties));
        }

        public static IRocketWebHostBuilder UseDependencyContext(
            this IRocketWebHostBuilder builder,
            DependencyContext dependencyContext,
            DiagnosticSource diagnosticSource = null)
        {
            return builder.LaunchWith(RocketBooster.ForDependencyContext(dependencyContext, diagnosticSource));
        }

        public static IRocketWebHostBuilder UseAppDomain(
            this IRocketWebHostBuilder builder,
            AppDomain appDomain,
            DiagnosticSource diagnosticSource = null)
        {
            return builder.LaunchWith(RocketBooster.ForAppDomain(appDomain, diagnosticSource));
        }

        public static IRocketWebHostBuilder UseAssemblies(
            this IRocketWebHostBuilder builder,
            IEnumerable<Assembly> assemblies,
            DiagnosticSource diagnosticSource = null)
        {
            return builder.LaunchWith(RocketBooster.ForAssemblies(assemblies, diagnosticSource));
        }

        private static void DefaultServices(IWebHostBuilder builder, WebHostBuilderContext context, IServiceCollection services)
        {
            var conventionalBuilder = GetOrCreateBuilder(builder);
            services.RemoveAll<IRocketServiceComposer>();
            services.AddSingleton(_ => conventionalBuilder.ApplicationServicesComposeDelegate(conventionalBuilder, context.Configuration, context.HostingEnvironment as Extensions.Hosting.IHostingEnvironment));
        }

        public static IRocketWebHostBuilder UseSystemServices(this IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var conventionalBuilder = GetOrCreateBuilder(builder);
                services.RemoveAll<IRocketServiceComposer>();
                services.AddSingleton(_ => conventionalBuilder.ApplicationAndSystemServicesComposeDelegate(conventionalBuilder, context.Configuration, context.HostingEnvironment as Extensions.Hosting.IHostingEnvironment));
            });
            return GetOrCreateBuilder(builder);
        }

        public static async Task<int> RunCli(this IWebHostBuilder builder)
        {
            using (var host = builder.Build())
            {
                try
                {
                    var context = host.Services.GetRequiredService<ICommandLineExecutor>();
                    if (context.IsDefaultCommand)
                    {
                        await host.StartAsync();
                        await host.WaitForShutdownAsync();
                        return 0;
                    }
                    else
                    {
                        var exec = host.Services.GetRequiredService<HostedServiceExecutor >();
                        await exec.StartAsync(CancellationToken.None);
                        var result = context.Execute(host.Services);
                        await exec.StopAsync(CancellationToken.None);
                        return result;
                    }
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

        internal static RocketWebHostBuilder GetConventionalWebHostBuilder(IWebHostBuilder builder)
        {
            return GetOrCreateBuilder(builder);
        }

        private static RocketWebHostBuilder GetOrCreateBuilder(IRocketWebHostBuilder builder)
        {
            return GetOrCreateBuilder(builder.Builder);
        }

        internal static RocketWebHostBuilder GetOrCreateBuilder(IWebHostBuilder builder)
        {
            if (builder is IRocketWebHostBuilder rb) builder = rb.Builder;
            if (!Builders.TryGetValue(builder, out var conventionalBuilder))
            {
                var diagnosticSource = new DiagnosticListener("Rocket.Surgery.Hosting");
                var logger = new DiagnosticLogger(diagnosticSource);
                var assemblyCandidateFinder = new DefaultAssemblyCandidateFinder(Array.Empty<Assembly>(), logger);
                var assemblyProvider = new DefaultAssemblyProvider(Array.Empty<Assembly>(), logger);
                var scanner = new BasicConventionScanner();
                conventionalBuilder = new RocketWebHostBuilder(builder, scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, new Dictionary<object, object>());

                var startupAssemblies = ((builder.GetSetting(WebHostDefaults.HostingStartupAssembliesKey) ?? "") + ";" +
                                         typeof(RocketHostingStartup).Assembly.GetName().Name).Trim(';');
                builder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, startupAssemblies);
                conventionalBuilder.ApplicationServicesComposeDelegate = (b, configuration, environment) => new RocketServiceComposer(
                    b.Scanner,
                    b.AssemblyProvider,
                    b.AssemblyCandidateFinder,
                    configuration,
                    environment,
                    b.DiagnosticSource);
                conventionalBuilder.ApplicationAndSystemServicesComposeDelegate = (b, configuration, environment) => new RocketApplicationServiceComposer(
                    b.Scanner,
                    b.AssemblyProvider,
                    b.AssemblyCandidateFinder,
                    configuration,
                    environment,
                    b.DiagnosticSource);
                builder.ConfigureServices((context, services) => DefaultServices(builder, context, services));
                Builders.Add(builder, conventionalBuilder);
            }

            return conventionalBuilder;
        }

        internal static RocketWebHostBuilder Swap(IWebHostBuilder builder, RocketWebHostBuilder newRocketBuilder)
        {
            Builders.Remove(builder);
            Builders.Add(builder, newRocketBuilder);
            return newRocketBuilder;
        }
    }
}
