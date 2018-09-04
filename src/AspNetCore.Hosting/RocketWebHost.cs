using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyModel;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public static class RocketWebHost
    {
        public static IRocketWebHostBuilder ForDependencyContext(
            DependencyContext dependencyContext,
            string[] arguments = null,
            DiagnosticSource diagnosticSource = null)
        {
            diagnosticSource = diagnosticSource ?? new DiagnosticListener("Rocket.Surgery.Hosting");
            var logger = new DiagnosticLogger(diagnosticSource);
            var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(dependencyContext, logger);
            var assemblyProvider = new DependencyContextAssemblyProvider(dependencyContext, logger);
            var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
            return new RocketWebHostBuilder(
                WebHost.CreateDefaultBuilder(),
                scanner,
                assemblyCandidateFinder,
                assemblyProvider,
                diagnosticSource,
                arguments);
        }

        public static IRocketWebHostBuilder ForAppDomain(
            AppDomain appDomain,
            string[] arguments = null,
            DiagnosticSource diagnosticSource = null)
        {
            diagnosticSource = diagnosticSource ?? new DiagnosticListener("Rocket.Surgery.Hosting");
            var logger = new DiagnosticLogger(diagnosticSource);

            var assemblyCandidateFinder = new AppDomainAssemblyCandidateFinder(appDomain, logger);
            var assemblyProvider = new AppDomainAssemblyProvider(appDomain, logger);
            var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
            return new RocketWebHostBuilder(
                WebHost.CreateDefaultBuilder(),
                scanner,
                assemblyCandidateFinder,
                assemblyProvider,
                diagnosticSource,
                arguments);
        }


        public static IRocketWebHostBuilder ForAssemblies(
            IEnumerable<Assembly> assemblies,
            string[] arguments = null,
            DiagnosticSource diagnosticSource = null)
        {
            diagnosticSource = diagnosticSource ?? new DiagnosticListener("Rocket.Surgery.Hosting");
            var logger = new DiagnosticLogger(diagnosticSource);

            var enumerable = assemblies as Assembly[] ?? assemblies.ToArray();
            var assemblyCandidateFinder = new DefaultAssemblyCandidateFinder(enumerable, logger);
            var assemblyProvider = new DefaultAssemblyProvider(enumerable, logger);
            var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
            return new RocketWebHostBuilder(
                WebHost.CreateDefaultBuilder(),
                scanner,
                assemblyCandidateFinder,
                assemblyProvider,
                diagnosticSource,
                arguments);
        }
    }
}
