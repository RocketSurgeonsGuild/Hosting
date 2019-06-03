using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyModel;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
//using Microsoft.Azure.WebJobs.Hosting;

namespace Rocket.Surgery.Hosting.Functions
{
    public static class RocketBooster
    {
        public static Func<IWebJobsBuilder, object, IRocketFunctionHostBuilder> ForDependencyContext(
            DependencyContext dependencyContext,
            DiagnosticSource diagnosticSource = null)
        {
            return (builder , startupInstance) =>
            {
                var b = RocketHostExtensions.GetOrCreateBuilder(builder, startupInstance, null);
                if (diagnosticSource != null)
                {
                    b = b.With(diagnosticSource);
                }

                var logger = new DiagnosticLogger(b.DiagnosticSource);
                var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(dependencyContext, logger);
                var assemblyProvider = new DependencyContextAssemblyProvider(dependencyContext, logger);
                var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
                return RocketHostExtensions.Swap(b, b
                        .With(assemblyCandidateFinder)
                        .With(assemblyProvider)
                        .With(scanner)
                );
            };
        }

        public static Func<IWebJobsBuilder, object, IRocketFunctionHostBuilder> For(DependencyContext dependencyContext, DiagnosticSource diagnosticSource = null)
        {
            return ForDependencyContext(dependencyContext, diagnosticSource);
        }

        public static Func<IWebJobsBuilder, object, IRocketFunctionHostBuilder> ForAppDomain(
            AppDomain appDomain,
            DiagnosticSource diagnosticSource = null)
        {
            return (builder, startupInstance) =>
            {
                var b = RocketHostExtensions.GetOrCreateBuilder(builder, startupInstance, null);
                if (diagnosticSource != null)
                {
                    b = b.With(diagnosticSource);
                }

                var logger = new DiagnosticLogger(b.DiagnosticSource);
                var assemblyCandidateFinder = new AppDomainAssemblyCandidateFinder(appDomain, logger);
                var assemblyProvider = new AppDomainAssemblyProvider(appDomain, logger);
                var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
                return RocketHostExtensions.Swap(b, b
                        .With(assemblyCandidateFinder)
                        .With(assemblyProvider)
                        .With(scanner)
                );
            };
        }

        public static Func<IWebJobsBuilder, object, IRocketFunctionHostBuilder> For(AppDomain appDomain, DiagnosticSource diagnosticSource = null)
        {
            return ForAppDomain(appDomain, diagnosticSource);
        }

        public static Func<IWebJobsBuilder, object, IRocketFunctionHostBuilder> ForAssemblies(
            IEnumerable<Assembly> assemblies,
            DiagnosticSource diagnosticSource = null)
        {
            return (builder, startupInstance) =>
            {
                var b = RocketHostExtensions.GetOrCreateBuilder(builder, startupInstance, null);
                if (diagnosticSource != null)
                {
                    b = b.With(diagnosticSource);
                }

                var logger = new DiagnosticLogger(b.DiagnosticSource);
                var enumerable = assemblies as Assembly[] ?? assemblies.ToArray();
                var assemblyCandidateFinder = new DefaultAssemblyCandidateFinder(enumerable, logger);
                var assemblyProvider = new DefaultAssemblyProvider(enumerable, logger);
                var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
                return RocketHostExtensions.Swap(b, b
                        .With(assemblyCandidateFinder)
                        .With(assemblyProvider)
                        .With(scanner)
                );
            };
        }

        public static Func<IWebJobsBuilder, object, IRocketFunctionHostBuilder> For(IEnumerable<Assembly> assemblies, DiagnosticSource diagnosticSource = null)
        {
            return ForAssemblies(assemblies, diagnosticSource);
        }
    }
}
