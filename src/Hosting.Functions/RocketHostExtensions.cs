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
    /// <summary>
    /// Class RocketHostExtensions.
    /// </summary>
    public static class RocketHostExtensions
    {
        private static readonly ConditionalWeakTable<IWebJobsBuilder, RocketFunctionHostBuilder> Builders = new ConditionalWeakTable<IWebJobsBuilder, RocketFunctionHostBuilder>();

        /// <summary>
        /// Uses the rocket Surgery.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="startupInstance">The startup instance.</param>
        /// <param name="action">The action.</param>
        /// <returns>IWebJobsBuilder.</returns>
        public static IWebJobsBuilder UseRocketSurgery(this IWebJobsBuilder builder, object startupInstance, Action<IRocketFunctionHostBuilder> action)
        {
            var internalBuilder = GetOrCreateBuilder(builder, startupInstance, null);
            action(internalBuilder);
            internalBuilder.Compose();
            return builder;
        }

        /// <summary>
        /// Uses the rocket Surgery.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="startupInstance">The startup instance.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="action">The action.</param>
        /// <returns>IWebJobsBuilder.</returns>
        public static IWebJobsBuilder UseRocketSurgery(this IWebJobsBuilder builder, object startupInstance, IRocketEnvironment environment, Action<IRocketFunctionHostBuilder> action)
        {
            var internalBuilder = GetOrCreateBuilder(builder, startupInstance, environment);
            action(internalBuilder);
            internalBuilder.Compose();
            return builder;
        }

        /// <summary>
        /// Uses the rocket booster.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="startupInstance">The startup instance.</param>
        /// <param name="func">The function.</param>
        /// <param name="action">The action.</param>
        /// <returns>IWebJobsBuilder.</returns>
        /// <exception cref="Exception">Something bad happened...</exception>
        public static IWebJobsBuilder UseRocketBooster(this IWebJobsBuilder builder, object startupInstance, Func<IWebJobsBuilder, object, IRocketFunctionHostBuilder> func, Action<IRocketFunctionHostBuilder> action)
        {
            var b = func(builder, startupInstance);
            action(b);
            if (!Builders.TryGetValue(builder, out var conventionalBuilder))
                throw new Exception("Something bad happened...");
            conventionalBuilder.Compose();
            return builder;
        }

        /// <summary>
        /// Uses the rocket booster.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="startupInstance">The startup instance.</param>
        /// <param name="func">The function.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="action">The action.</param>
        /// <returns>IWebJobsBuilder.</returns>
        /// <exception cref="Exception">Something bad happened...</exception>
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

        /// <summary>
        /// Launches the with.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="startupInstance">The startup instance.</param>
        /// <param name="func">The function.</param>
        /// <param name="action">The action.</param>
        /// <returns>IWebJobsBuilder.</returns>
        /// <exception cref="Exception">Something bad happened...</exception>
        public static IWebJobsBuilder LaunchWith(this IWebJobsBuilder builder, object startupInstance, Func<IWebJobsBuilder, object, IRocketFunctionHostBuilder> func, Action<IRocketFunctionHostBuilder> action)
        {
            var b = func(builder, startupInstance);
            action(b);
            if (!Builders.TryGetValue(builder, out var conventionalBuilder))
                throw new Exception("Something bad happened...");
            conventionalBuilder.Compose();
            return builder;
        }

        /// <summary>
        /// Launches the with.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="startupInstance">The startup instance.</param>
        /// <param name="func">The function.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="action">The action.</param>
        /// <returns>IWebJobsBuilder.</returns>
        /// <exception cref="Exception">Something bad happened...</exception>
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

        /// <summary>
        /// Uses the scanner.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="scanner">The scanner.</param>
        /// <returns>IRocketFunctionHostBuilder.</returns>
        public static IRocketFunctionHostBuilder UseScanner(this IRocketFunctionHostBuilder builder, IConventionScanner scanner)
        {
            return Swap(builder, GetOrCreateBuilder(builder, builder.FunctionsAssembly, null).With(scanner));
        }

        /// <summary>
        /// Uses the functions assembly.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assembly">The assembly.</param>
        /// <returns>IRocketFunctionHostBuilder.</returns>
        public static IRocketFunctionHostBuilder UseFunctionsAssembly(this IRocketFunctionHostBuilder builder, Assembly assembly)
        {
            return Swap(builder, GetOrCreateBuilder(builder, builder.FunctionsAssembly, null).With(assembly));
        }

        /// <summary>
        /// Uses the assembly candidate finder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assemblyCandidateFinder">The assembly candidate finder.</param>
        /// <returns>IRocketFunctionHostBuilder.</returns>
        public static IRocketFunctionHostBuilder UseAssemblyCandidateFinder(this IRocketFunctionHostBuilder builder, IAssemblyCandidateFinder assemblyCandidateFinder)
        {
            return Swap(builder, GetOrCreateBuilder(builder, builder.FunctionsAssembly, null).With(assemblyCandidateFinder));
        }

        /// <summary>
        /// Uses the assembly provider.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assemblyProvider">The assembly provider.</param>
        /// <returns>IRocketFunctionHostBuilder.</returns>
        public static IRocketFunctionHostBuilder UseAssemblyProvider(this IRocketFunctionHostBuilder builder, IAssemblyProvider assemblyProvider)
        {
            return Swap(builder, GetOrCreateBuilder(builder, builder.FunctionsAssembly, null).With(assemblyProvider));
        }

        /// <summary>
        /// Uses the diagnostic source.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="diagnosticSource">The diagnostic source.</param>
        /// <returns>IRocketFunctionHostBuilder.</returns>
        public static IRocketFunctionHostBuilder UseDiagnosticSource(this IRocketFunctionHostBuilder builder, DiagnosticSource diagnosticSource)
        {
            return Swap(builder, GetOrCreateBuilder(builder, builder.FunctionsAssembly, null).With(diagnosticSource));
        }

        /// <summary>
        /// Uses the environment.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>IRocketFunctionHostBuilder.</returns>
        public static IRocketFunctionHostBuilder UseEnvironment(this IRocketFunctionHostBuilder builder, IRocketEnvironment environment)
        {
            return Swap(builder, GetOrCreateBuilder(builder, builder.FunctionsAssembly, null).With(environment));
        }

        /// <summary>
        /// Uses the dependency context.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="dependencyContext">The dependency context.</param>
        /// <param name="diagnosticSource">The diagnostic source.</param>
        /// <returns>IRocketFunctionHostBuilder.</returns>
        public static IRocketFunctionHostBuilder UseDependencyContext(
            this IRocketFunctionHostBuilder builder,
            DependencyContext dependencyContext,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForDependencyContext(dependencyContext, diagnosticSource)(builder.Builder, builder.FunctionsAssembly);
        }

        /// <summary>
        /// Uses the application domain.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="appDomain">The application domain.</param>
        /// <param name="diagnosticSource">The diagnostic source.</param>
        /// <returns>IRocketFunctionHostBuilder.</returns>
        public static IRocketFunctionHostBuilder UseAppDomain(
            this IRocketFunctionHostBuilder builder,
            AppDomain appDomain,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForAppDomain(appDomain, diagnosticSource)(builder.Builder, builder.FunctionsAssembly);
        }

        /// <summary>
        /// Uses the assemblies.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="diagnosticSource">The diagnostic source.</param>
        /// <returns>IRocketFunctionHostBuilder.</returns>
        public static IRocketFunctionHostBuilder UseAssemblies(
            this IRocketFunctionHostBuilder builder,
            IEnumerable<Assembly> assemblies,
            DiagnosticSource diagnosticSource = null)
        {
            return RocketBooster.ForAssemblies(assemblies, diagnosticSource)(builder.Builder, builder.FunctionsAssembly);
        }

        /// <summary>
        /// Gets the or create builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="startupInstance">The startup instance.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>RocketFunctionHostBuilder.</returns>
        internal static RocketFunctionHostBuilder GetOrCreateBuilder(IRocketFunctionHostBuilder builder, object startupInstance, IRocketEnvironment environment)
        {
            return GetOrCreateBuilder(builder.Builder, startupInstance, environment);
        }

        /// <summary>
        /// Gets the or create builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="startupInstance">The startup instance.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>RocketFunctionHostBuilder.</returns>
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
                var properties = new ServiceProviderDictionary();
                var scanner = new SimpleConventionScanner(assemblyCandidateFinder, properties, logger);
                conventionalBuilder = new RocketFunctionHostBuilder(builder, functionsAssembly, startupInstance, environment, scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, properties);
                Builders.Add(builder, conventionalBuilder);
            }

            return conventionalBuilder;
        }

        /// <summary>
        /// Swaps the specified builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="newRocketFunctionBuilder">The new rocket function builder.</param>
        /// <returns>RocketFunctionHostBuilder.</returns>
        internal static RocketFunctionHostBuilder Swap(IRocketFunctionHostBuilder builder, RocketFunctionHostBuilder newRocketFunctionBuilder)
        {
            Builders.Remove(builder.Builder);
            Builders.Add(builder.Builder, newRocketFunctionBuilder);
            return newRocketFunctionBuilder;
        }
    }
}
