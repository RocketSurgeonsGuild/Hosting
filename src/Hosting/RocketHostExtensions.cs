using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
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

        public static IRocketHostBuilder UseRocketBooster(this IRocketHostBuilder builder, Func<IRocketHostBuilder, IRocketHostBuilder> func)
        {
            return func(builder);
        }

        public static IRocketHostBuilder LaunchWith(this IRocketHostBuilder builder, Func<IRocketHostBuilder, IRocketHostBuilder> func)
        {
            return func(builder);
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
            return RocketBooster.ForDependencyContext(dependencyContext, diagnosticSource)(builder);
        }

        public static IRocketHostBuilder UseAppDomain(
            this IRocketHostBuilder builder,
            AppDomain appDomain,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForAppDomain(appDomain, diagnosticSource)(builder);
        }

        public static IRocketHostBuilder UseAssemblies(
            this IRocketHostBuilder builder,
            IEnumerable<Assembly> assemblies,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForAssemblies(assemblies, diagnosticSource)(builder);
        }

        private static void DefaultServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
        {
            var conventionalBuilder = GetOrCreateBuilder(builder);
            builder.UseServiceProviderFactory(
                new ServicesBuilderServiceProviderFactory(collection =>
                    new ServicesBuilder(
                        conventionalBuilder.Scanner,
                        conventionalBuilder.AssemblyProvider,
                        conventionalBuilder.AssemblyCandidateFinder,
                        collection,
                        context.Configuration,
                        context.HostingEnvironment,
                        conventionalBuilder.DiagnosticSource,
                        conventionalBuilder.Properties
                    )
                )
            );
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
                    .ConfigureServices((context, services) => DefaultServices(builder, context, services));
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
    }
}
