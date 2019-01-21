using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Rocket.Surgery.AspNetCore.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Hosting
{
    public static class RocketWebHostExtensions
    {
        private static readonly ConditionalWeakTable<IWebHostBuilder, RocketWebHostBuilder> Builders = new ConditionalWeakTable<IWebHostBuilder, RocketWebHostBuilder>();
        public static IRocketWebHostBuilder UseConventional(this IWebHostBuilder builder)
        {
            return GetOrCreateBuilder(builder);
        }

        public static IRocketWebHostBuilder UseConventionalScanner(this IRocketWebHostBuilder builder, IConventionScanner scanner)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(scanner));
        }

        public static IRocketWebHostBuilder UseConventionalAssemblyCandidateFinder(this IRocketWebHostBuilder builder, IAssemblyCandidateFinder assemblyCandidateFinder)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(assemblyCandidateFinder));
        }

        public static IRocketWebHostBuilder UseConventionalAssemblyProvider(this IRocketWebHostBuilder builder, IAssemblyProvider assemblyProvider)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(assemblyProvider));
        }

        public static IRocketWebHostBuilder UseConventionalDiagnosticSource(this IRocketWebHostBuilder builder, DiagnosticSource diagnosticSource)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(diagnosticSource));
        }

        public static IRocketWebHostBuilder UseConventionalProperties(this IRocketWebHostBuilder builder, IDictionary<object, object> properties)
        {
            return Swap(builder, GetOrCreateBuilder(builder).With(properties));
        }

        public static IRocketWebHostBuilder UseConventionalDependencyContext(
            this IWebHostBuilder builder,
            DependencyContext dependencyContext,
            DiagnosticSource diagnosticSource = null)
        {
            var b = GetOrCreateBuilder(builder);
            if (diagnosticSource != null)
            {
                b = b.With(diagnosticSource);
            }

            var logger = new DiagnosticLogger(b.DiagnosticSource);
            var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(dependencyContext, logger);
            var assemblyProvider = new DependencyContextAssemblyProvider(dependencyContext, logger);
            var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
            return Swap(
                builder, b
                    .With(assemblyCandidateFinder)
                    .With(assemblyProvider)
                    .With(scanner)
                );
        }

        public static IRocketWebHostBuilder UseDependencyContext(
            this IRocketWebHostBuilder builder,
            DependencyContext dependencyContext,
            DiagnosticSource diagnosticSource = null)
        {
            return builder.UseConventionalDependencyContext(dependencyContext, diagnosticSource);
        }

        public static IRocketWebHostBuilder UseConventionalAppDomain(
            this IWebHostBuilder builder,
            AppDomain appDomain,
            DiagnosticSource diagnosticSource = null)
        {
            var b = GetOrCreateBuilder(builder);
            if (diagnosticSource != null)
            {
                b = b.With(diagnosticSource);
            }

            var logger = new DiagnosticLogger(b.DiagnosticSource);
            var assemblyCandidateFinder = new AppDomainAssemblyCandidateFinder(appDomain, logger);
            var assemblyProvider = new AppDomainAssemblyProvider(appDomain, logger);
            var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
            return Swap(
                builder, b
                    .With(assemblyCandidateFinder)
                    .With(assemblyProvider)
                    .With(scanner)
            );
        }

        public static IRocketWebHostBuilder UseAppDomain(
            this IRocketWebHostBuilder builder,
            AppDomain appDomain,
            DiagnosticSource diagnosticSource = null)
        {
            return builder.UseConventionalAppDomain(appDomain, diagnosticSource);
        }


        public static IRocketWebHostBuilder UseConventionalAssemblies(
            this IWebHostBuilder builder,
            IEnumerable<Assembly> assemblies,
            DiagnosticSource diagnosticSource = null)
        {
            var b = GetOrCreateBuilder(builder);
            if (diagnosticSource != null)
            {
                b = b.With(diagnosticSource);
            }

            var logger = new DiagnosticLogger(b.DiagnosticSource);
            var enumerable = assemblies as Assembly[] ?? assemblies.ToArray();
            var assemblyCandidateFinder = new DefaultAssemblyCandidateFinder(enumerable, logger);
            var assemblyProvider = new DefaultAssemblyProvider(enumerable, logger);
            var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
            return Swap(
                builder, b
                    .With(assemblyCandidateFinder)
                    .With(assemblyProvider)
                    .With(scanner)
            );
        }

        public static IRocketWebHostBuilder UseAssemblies(
            this IRocketWebHostBuilder builder,
            IEnumerable<Assembly> assemblies,
            DiagnosticSource diagnosticSource = null)
        {
            return builder.UseConventionalAssemblies(assemblies, diagnosticSource);
        }

        internal static IRocketWebHostBuilder UseServices(this IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var conventionalBuilder = GetOrCreateBuilder(builder);
                services.RemoveAll<IRocketServiceComposer>();
                services.AddSingleton(_ => conventionalBuilder.ApplicationServicesComposeDelegate(conventionalBuilder, context.Configuration, context.HostingEnvironment as Extensions.Hosting.IHostingEnvironment));
            });
            return GetOrCreateBuilder(builder);
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
            if (!Builders.TryGetValue(builder, out var conventionalBuilder))
            {
                var diagnosticSource = new DiagnosticListener("Rocket.Surgery.Hosting");
                var dependencyContext = DependencyContext.Default;
                var logger = new DiagnosticLogger(diagnosticSource);
                var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(dependencyContext, logger);
                var assemblyProvider = new DependencyContextAssemblyProvider(dependencyContext, logger);
                var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
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
                Builders.Add(builder, conventionalBuilder);
                conventionalBuilder.UseServices();
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
