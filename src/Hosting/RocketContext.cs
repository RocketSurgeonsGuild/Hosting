using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetEscapades.Configuration.Yaml;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.DependencyInjection;
using ConfigurationBuilder = Rocket.Surgery.Extensions.Configuration.ConfigurationBuilder;
using IConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;

namespace Rocket.Surgery.Hosting
{
    /// <summary>
    /// Class RocketContext.
    /// </summary>
    class RocketContext
    {
        private readonly IHostBuilder _hostBuilder;
        private string[] _args;
        private ICommandLineExecutor _exec;

        /// <summary>
        /// Initializes a new instance of the <see cref="RocketContext"/> class.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        public RocketContext(IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
        }

        /// <summary>
        /// Configures the cli.
        /// </summary>
        /// <param name="configurationBuilder">The configuration builder.</param>
        public void ConfigureCli(IConfigurationBuilder configurationBuilder)
        {
            var rocketHostBuilder = RocketHostExtensions.GetConventionalHostBuilder(_hostBuilder);
            var clb = new CommandLineBuilder(
                rocketHostBuilder.Scanner,
                rocketHostBuilder.AssemblyProvider,
                rocketHostBuilder.AssemblyCandidateFinder,
                rocketHostBuilder.Logger,
                rocketHostBuilder.Properties
            );

            _exec = clb.Build().Parse(_args ?? Array.Empty<string>());
            _args = _exec.ApplicationState.RemainingArguments ?? Array.Empty<string>();
            configurationBuilder.AddApplicationState(_exec.ApplicationState);
            rocketHostBuilder.Properties.Add(typeof(ICommandLineExecutor), _exec);
        }

        /// <summary>
        /// Captures the arguments.
        /// </summary>
        /// <param name="configurationBuilder">The configuration builder.</param>
        public void CaptureArguments(IConfigurationBuilder configurationBuilder)
        {
            var commandLineSource = configurationBuilder.Sources.OfType<CommandLineConfigurationSource>()
                .FirstOrDefault();
            if (commandLineSource != null)
            {
                _args = commandLineSource.Args.ToArray();
            }
        }

        /// <summary>
        /// Replaces the arguments.
        /// </summary>
        /// <param name="configurationBuilder">The configuration builder.</param>
        public void ReplaceArguments(IConfigurationBuilder configurationBuilder)
        {
            var commandLineSource = configurationBuilder.Sources.OfType<CommandLineConfigurationSource>()
                .FirstOrDefault();
            if (commandLineSource != null)
            {
                commandLineSource.Args = _args;
            }
        }

        /// <summary>
        /// Configures the application configuration.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="configurationBuilder">The configuration builder.</param>
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
                context.HostingEnvironment.Convert(),
                context.Configuration,
                configurationBuilder,
                rocketHostBuilder.Logger,
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

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="services">The services.</param>
        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var rocketHostBuilder = RocketHostExtensions.GetConventionalHostBuilder(_hostBuilder);
            services.AddSingleton(rocketHostBuilder.AssemblyCandidateFinder);
            services.AddSingleton(rocketHostBuilder.AssemblyProvider);
            services.AddSingleton(rocketHostBuilder.Scanner);
#if !(NETSTANDARD2_0 || NETCOREAPP2_1)
            services.AddHealthChecks();
#endif
        }

        /// <summary>
        /// Defaults the services.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="services">The services.</param>
        public void DefaultServices(HostBuilderContext context, IServiceCollection services)
        {
            var conventionalBuilder = RocketHostExtensions.GetConventionalHostBuilder(_hostBuilder);
            _hostBuilder.UseServiceProviderFactory(
                new ServicesBuilderServiceProviderFactory(collection =>
                    new ServicesBuilder(
                        conventionalBuilder.Scanner,
                        conventionalBuilder.AssemblyProvider,
                        conventionalBuilder.AssemblyCandidateFinder,
                        collection,
                        context.Configuration,
                        context.HostingEnvironment.Convert(),
                        conventionalBuilder.Logger,
                        conventionalBuilder.Properties
                    )
                )
            );
        }
    }
}
