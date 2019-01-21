using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Rocket.Surgery.AspNetCore.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Hosting
{
    public static class RocketBooster
    {
        public static Func<IWebHostBuilder, IRocketWebHostBuilder> ForDependencyContext(
            DependencyContext dependencyContext,
            DiagnosticSource diagnosticSource = null)
        {
            return builder =>
            {
                var b = RocketWebHostExtensions.GetOrCreateBuilder(builder);
                if (diagnosticSource != null)
                {
                    b = b.With(diagnosticSource);
                }

                var logger = new DiagnosticLogger(b.DiagnosticSource);
                var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(dependencyContext, logger);
                var assemblyProvider = new DependencyContextAssemblyProvider(dependencyContext, logger);
                var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
                return RocketWebHostExtensions.Swap(
                    builder, b
                        .With(assemblyCandidateFinder)
                        .With(assemblyProvider)
                        .With(scanner)
                );
            };
        }

        public static Func<IWebHostBuilder, IRocketWebHostBuilder> For(DependencyContext dependencyContext, DiagnosticSource diagnosticSource = null)
        {
            return ForDependencyContext(dependencyContext, diagnosticSource);
        }

        public static Func<IWebHostBuilder, IRocketWebHostBuilder> ForAppDomain(
            AppDomain appDomain,
            DiagnosticSource diagnosticSource = null)
        {
            return builder =>
            {
                var b = RocketWebHostExtensions.GetOrCreateBuilder(builder);
                if (diagnosticSource != null)
                {
                    b = b.With(diagnosticSource);
                }

                var logger = new DiagnosticLogger(b.DiagnosticSource);
                var assemblyCandidateFinder = new AppDomainAssemblyCandidateFinder(appDomain, logger);
                var assemblyProvider = new AppDomainAssemblyProvider(appDomain, logger);
                var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
                return RocketWebHostExtensions.Swap(
                    builder, b
                        .With(assemblyCandidateFinder)
                        .With(assemblyProvider)
                        .With(scanner)
                );
            };
        }

        public static Func<IWebHostBuilder, IRocketWebHostBuilder> For(AppDomain appDomain, DiagnosticSource diagnosticSource = null)
        {
            return ForAppDomain(appDomain, diagnosticSource);
        }


        public static Func<IWebHostBuilder, IRocketWebHostBuilder> ForAssemblies(
            IEnumerable<Assembly> assemblies,
            DiagnosticSource diagnosticSource = null)
        {
            return builder =>
            {
                var b = RocketWebHostExtensions.GetOrCreateBuilder(builder);
                if (diagnosticSource != null)
                {
                    b = b.With(diagnosticSource);
                }

                var logger = new DiagnosticLogger(b.DiagnosticSource);
                var enumerable = assemblies as Assembly[] ?? assemblies.ToArray();
                var assemblyCandidateFinder = new DefaultAssemblyCandidateFinder(enumerable, logger);
                var assemblyProvider = new DefaultAssemblyProvider(enumerable, logger);
                var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
                return RocketWebHostExtensions.Swap(
                    builder, b
                        .With(assemblyCandidateFinder)
                        .With(assemblyProvider)
                        .With(scanner)
                );
            };
        }

        public static Func<IWebHostBuilder, IRocketWebHostBuilder> For(IEnumerable<Assembly> assemblies, DiagnosticSource diagnosticSource = null)
        {
            return ForAssemblies(assemblies, diagnosticSource);
        }
    }
}
