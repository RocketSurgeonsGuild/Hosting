using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
//using Microsoft.Azure.WebJobs.Hosting;

namespace Rocket.Surgery.Hosting.Functions
{
    public static class RocketHostExtensions
    {
        private static readonly ConditionalWeakTable<IWebJobsBuilder, RocketFunctionHostBuilder> Builders = new ConditionalWeakTable<IWebJobsBuilder, RocketFunctionHostBuilder>();

        public static IWebJobsBuilder UseRocketSurgey(this IWebJobsBuilder builder, object startupInstance, Action<IRocketFunctionHostBuilder> action)
        {
            var internalBuilder = GetOrCreateBuilder(builder, startupInstance, null);
            action(internalBuilder);
            internalBuilder.Compose();
            return builder;
        }
        public static IWebJobsBuilder UseRocketSurgey(this IWebJobsBuilder builder, object startupInstance, IRocketEnvironment environment, Action<IRocketFunctionHostBuilder> action)
        {
            var internalBuilder = GetOrCreateBuilder(builder, startupInstance, environment);
            action(internalBuilder);
            internalBuilder.Compose();
            return builder;
        }

        public static IWebJobsBuilder UseRocketBooster(this IWebJobsBuilder builder, object startupInstance, Func<IWebJobsBuilder, object, IRocketFunctionHostBuilder> func, Action<IRocketFunctionHostBuilder> action)
        {
            var b = func(builder, startupInstance);
            action(b);
            if (!Builders.TryGetValue(builder, out var conventionalBuilder))
                throw new Exception("Something bad happened...");
            conventionalBuilder.Compose();
            return builder;
        }

        public static IWebJobsBuilder UseRocketBooster(this IWebJobsBuilder builder, object startupInstance, Func<IWebJobsBuilder, object, IRocketFunctionHostBuilder> func, IRocketEnvironment environment, Action<IRocketFunctionHostBuilder> action)
        {
            var b = func(builder, startupInstance);
            b.UseEnvironment(environment);
            action(b);
            if (!Builders.TryGetValue(builder, out var conventionalBuilder))
                throw new Exception("Something bad happened...");
            conventionalBuilder.Compose();
            return builder;
        }

        public static IWebJobsBuilder LaunchWith(this IWebJobsBuilder builder, object startupInstance, Func<IWebJobsBuilder, object, IRocketFunctionHostBuilder> func, Action<IRocketFunctionHostBuilder> action)
        {
            var b = func(builder, startupInstance);
            action(b);
            if (!Builders.TryGetValue(builder, out var conventionalBuilder))
                throw new Exception("Something bad happened...");
            conventionalBuilder.Compose();
            return builder;
        }

        public static IWebJobsBuilder LaunchWith(this IWebJobsBuilder builder, object startupInstance, Func<IWebJobsBuilder, object, IRocketFunctionHostBuilder> func, IRocketEnvironment environment, Action<IRocketFunctionHostBuilder> action)
        {
            var b = func(builder, startupInstance);
            b.UseEnvironment(environment);
            action(b);
            if (!Builders.TryGetValue(builder, out var conventionalBuilder))
                throw new Exception("Something bad happened...");
            conventionalBuilder.Compose();
            return builder;
        }

        public static IRocketFunctionHostBuilder UseScanner(this IRocketFunctionHostBuilder builder, IConventionScanner scanner)
        {
            return Swap(builder, GetOrCreateBuilder(builder, builder.FunctionsAssembly, null).With(scanner));
        }

        public static IRocketFunctionHostBuilder UseFunctionsAssembly(this IRocketFunctionHostBuilder builder, Assembly assembly)
        {
            return Swap(builder, GetOrCreateBuilder(builder, builder.FunctionsAssembly, null).With(assembly));
        }

        public static IRocketFunctionHostBuilder UseAssemblyCandidateFinder(this IRocketFunctionHostBuilder builder, IAssemblyCandidateFinder assemblyCandidateFinder)
        {
            return Swap(builder, GetOrCreateBuilder(builder, builder.FunctionsAssembly, null).With(assemblyCandidateFinder));
        }

        public static IRocketFunctionHostBuilder UseAssemblyProvider(this IRocketFunctionHostBuilder builder, IAssemblyProvider assemblyProvider)
        {
            return Swap(builder, GetOrCreateBuilder(builder, builder.FunctionsAssembly, null).With(assemblyProvider));
        }

        public static IRocketFunctionHostBuilder UseDiagnosticSource(this IRocketFunctionHostBuilder builder, DiagnosticSource diagnosticSource)
        {
            return Swap(builder, GetOrCreateBuilder(builder, builder.FunctionsAssembly, null).With(diagnosticSource));
        }

        public static IRocketFunctionHostBuilder UseEnvironment(this IRocketFunctionHostBuilder builder, IRocketEnvironment environment)
        {
            return Swap(builder, GetOrCreateBuilder(builder, builder.FunctionsAssembly, null).With(environment));
        }

        public static IRocketFunctionHostBuilder UseDependencyContext(
            this IRocketFunctionHostBuilder builder,
            DependencyContext dependencyContext,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForDependencyContext(dependencyContext, diagnosticSource)(builder.Builder, builder.FunctionsAssembly);
        }

        public static IRocketFunctionHostBuilder UseAppDomain(
            this IRocketFunctionHostBuilder builder,
            AppDomain appDomain,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForAppDomain(appDomain, diagnosticSource)(builder.Builder, builder.FunctionsAssembly);
        }

        public static IRocketFunctionHostBuilder UseAssemblies(
            this IRocketFunctionHostBuilder builder,
            IEnumerable<Assembly> assemblies,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForAssemblies(assemblies, diagnosticSource)(builder.Builder, builder.FunctionsAssembly);
        }

        internal static RocketFunctionHostBuilder GetOrCreateBuilder(IRocketFunctionHostBuilder builder, object startupInstance, IRocketEnvironment environment)
        {
            return GetOrCreateBuilder(builder.Builder, startupInstance, environment);
        }

        internal static RocketFunctionHostBuilder GetOrCreateBuilder(IWebJobsBuilder builder, object startupInstance, IRocketEnvironment environment)
        {
            if (!Builders.TryGetValue(builder, out var conventionalBuilder))
            {
                var diagnosticSource = new DiagnosticListener("Rocket.Surgery.Hosting");
                var functionsAssembly = startupInstance.GetType().Assembly;

                var location = Path.GetDirectoryName(functionsAssembly.Location);
                DependencyContext dependencyContext = null;
                while (dependencyContext == null && !string.IsNullOrEmpty(location))
                {
                    var depsFilePath = Path.Combine(location, functionsAssembly.GetName().Name + ".deps.json");
                    if (File.Exists(depsFilePath))
                    {
                        using (var stream = File.Open(depsFilePath, FileMode.Open, FileAccess.Read))
                        {
                            dependencyContext = new DependencyContextJsonReader().Read(stream);
                            break;
                        }
                    }
                    location = Path.GetDirectoryName(location);
                }
                var logger = new DiagnosticLogger(diagnosticSource);
                var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(dependencyContext, logger);
                var assemblyProvider = new DependencyContextAssemblyProvider(dependencyContext, logger);
                var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
                conventionalBuilder = new RocketFunctionHostBuilder(builder, functionsAssembly, startupInstance, environment, scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, new Dictionary<object, object>());
                Builders.Add(builder, conventionalBuilder);
            }

            return conventionalBuilder;
        }

        internal static RocketFunctionHostBuilder Swap(IRocketFunctionHostBuilder builder, RocketFunctionHostBuilder newRocketFunctionBuilder)
        {
            Builders.Remove(builder.Builder);
            Builders.Add(builder.Builder, newRocketFunctionBuilder);
            return newRocketFunctionBuilder;
        }
    }
}
