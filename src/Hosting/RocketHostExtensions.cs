using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using NetEscapades.Configuration.Yaml;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Hosting;
using ConfigurationBuilder = Rocket.Surgery.Extensions.Configuration.ConfigurationBuilder;
using IConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting
{
    public static class RocketHostExtensions
    {
        private static readonly ConditionalWeakTable<IHostBuilder, RocketHostBuilder> Builders = new ConditionalWeakTable<IHostBuilder, RocketHostBuilder>();

        public static IRocketHostBuilder UseRocketBooster(this IHostBuilder builder, Func<IHostBuilder, IRocketHostBuilder> func)
        {
            return func(builder);
        }

        public static IRocketHostBuilder LaunchWith(this IHostBuilder builder, Func<IHostBuilder, IRocketHostBuilder> func)
        {
            return func(builder);
        }

        public static IRocketHostBuilder UseConventional(this IHostBuilder builder)
        {
            return GetOrCreateBuilder(builder);
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
            return builder.LaunchWith(RocketBooster.ForDependencyContext(dependencyContext, diagnosticSource));
        }

        public static IRocketHostBuilder UseAppDomain(
            this IRocketHostBuilder builder,
            AppDomain appDomain,
            DiagnosticSource diagnosticSource = null)
        {
            return builder.LaunchWith(RocketBooster.ForAppDomain(appDomain, diagnosticSource));
        }

        public static IRocketHostBuilder UseAssemblies(
            this IRocketHostBuilder builder,
            IEnumerable<Assembly> assemblies,
            DiagnosticSource diagnosticSource = null)
        {
            return builder.LaunchWith(RocketBooster.ForAssemblies(assemblies, diagnosticSource));
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

        private static RocketHostBuilder GetOrCreateBuilder(IRocketHostBuilder builder)
        {
            return GetOrCreateBuilder(builder.Builder);
        }

        internal static RocketHostBuilder GetOrCreateBuilder(IHostBuilder builder)
        {
            if (builder is IRocketHostBuilder rb) builder = rb.Builder;
            if (!Builders.TryGetValue(builder, out var conventionalBuilder))
            {
                var diagnosticSource = new DiagnosticListener("Rocket.Surgery.Hosting");
                var dependencyContext = DependencyContext.Default;
                var logger = new DiagnosticLogger(diagnosticSource);
                var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(dependencyContext, logger);
                var assemblyProvider = new DependencyContextAssemblyProvider(dependencyContext, logger);
                var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
                conventionalBuilder = new RocketHostBuilder(builder, scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, builder.Properties);

                var host = new RocketHost(builder);
                builder
                    .ConfigureHostConfiguration(host.CaptureArguments)
                    .ConfigureAppConfiguration(host.CaptureArguments)
                    .ConfigureAppConfiguration(host.ConfigureAppConfiguration)
                    .ConfigureServices(host.ConfigureServices)
                    .ConfigureServices((context, services) => DefaultServices(builder, context, services));
                Builders.Add(builder, conventionalBuilder);

            }

            return conventionalBuilder;
        }

        internal static RocketHostBuilder Swap(IHostBuilder builder, RocketHostBuilder newRocketBuilder)
        {
            Builders.Remove(builder);
            Builders.Add(builder, newRocketBuilder);
            return newRocketBuilder;
        }
    }

    class RocketHost
    {
        private readonly IHostBuilder _hostBuilder;
        private string[] _args;

        public RocketHost(IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
        }

        public void CaptureArguments(IConfigurationBuilder configurationBuilder)
        {
            var commandLineSource = configurationBuilder.Sources.OfType<CommandLineConfigurationSource>()
                .FirstOrDefault();
            if (commandLineSource != null)
            {
                _args = commandLineSource.Args.ToArray();
            }
        }

        public void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder configurationBuilder)
        {
            var rocketHostBuilder = RocketHostExtensions.GetConventionalHostBuilder(_hostBuilder);
            InsertConfigurationSourceAfter(
                configurationBuilder.Sources,
                sources => sources.OfType<JsonConfigurationSource>().FirstOrDefault(x => x.Path == "appsettings.json"),
                (source) => new YamlConfigurationSource()
                {
                    Path = "appsettings.yml",
                    FileProvider = source.FileProvider,
                    Optional = true,
                    ReloadOnChange = true,
                });
            InsertConfigurationSourceAfter(
                configurationBuilder.Sources,
                sources => sources.OfType<JsonConfigurationSource>().FirstOrDefault(x =>
                    string.Equals(x.Path, $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                        StringComparison.OrdinalIgnoreCase)),
                (source) => new YamlConfigurationSource()
                {
                    Path = $"appsettings.{context.HostingEnvironment.EnvironmentName}.yml",
                    FileProvider = source.FileProvider,
                    Optional = true,
                    ReloadOnChange = true,
                });

            var cb = new ConfigurationBuilder(
                rocketHostBuilder.Scanner,
                context.HostingEnvironment,
                context.Configuration,
                configurationBuilder,
                rocketHostBuilder.DiagnosticSource,
                rocketHostBuilder.Properties);
            cb.Build();

            MoveConfigurationSourceToEnd(configurationBuilder.Sources,
                sources => sources.OfType<JsonConfigurationSource>().Where(x =>
                    string.Equals(x.Path, "secrets.json", StringComparison.OrdinalIgnoreCase)));
            MoveConfigurationSourceToEnd(configurationBuilder.Sources,
                sources => sources.OfType<EnvironmentVariablesConfigurationSource>());
            MoveConfigurationSourceToEnd(configurationBuilder.Sources,
                sources => sources.OfType<CommandLineConfigurationSource>());
        }

        private static void InsertConfigurationSourceAfter<T>(IList<IConfigurationSource> sources, Func<IList<IConfigurationSource>, T> getSource, Func<T, IConfigurationSource> createSourceFrom)
            where T : IConfigurationSource
        {
            var source = getSource(sources);
            if (source != null)
            {
                var index = sources.IndexOf(source);
                sources.Insert(index + 1, createSourceFrom(source));
            }
        }

        private static void MoveConfigurationSourceToEnd<T>(IList<IConfigurationSource> sources, Func<IList<IConfigurationSource>, IEnumerable<T>> getSource)
            where T : IConfigurationSource
        {
            var otherSources = getSource(sources).ToArray();
            if (otherSources.Any())
            {
                foreach (var other in otherSources)
                {
                    sources.Remove(other);
                }
                foreach (var other in otherSources)
                {
                    sources.Add(other);
                }
            }
        }

        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var rocketHostBuilder = RocketHostExtensions.GetConventionalHostBuilder(_hostBuilder);
            services.AddSingleton<IRocketHostingContext>(_ => new RocketHostingContext(rocketHostBuilder, _args ?? Array.Empty<string>()));
        }
    }
}
