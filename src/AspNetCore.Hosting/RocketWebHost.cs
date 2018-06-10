using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rocket.Surgery.AspNetCore.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Rocket.Surgery.Hosting
{
    public static class RocketWebHost
    {
        public static IRocketWebHostBuilder ForDependencyContext(
            DependencyContext dependencyContext,
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
                diagnosticSource);
        }

        public static IRocketWebHostBuilder ForAppDomain(
            AppDomain appDomain,
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
                diagnosticSource);
        }


        public static IRocketWebHostBuilder ForAssemblies(
            IEnumerable<Assembly> assemblies,
            DiagnosticSource diagnosticSource = null)
        {
            diagnosticSource = diagnosticSource ?? new DiagnosticListener("Rocket.Surgery.Hosting");
            var logger = new DiagnosticLogger(diagnosticSource);

            var enumerable = assemblies as Assembly[] ?? assemblies.ToArray();
            var assemblyCandidateFinder = new DefaultAssemblyCandidateFinder(enumerable, logger);
            var assemblyProvider = new DefaultAssemblyProvider(enumerable, logger);
            var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
            return new RocketWebHostBuilder(WebHost.CreateDefaultBuilder(), scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource);
        }
    }
}
