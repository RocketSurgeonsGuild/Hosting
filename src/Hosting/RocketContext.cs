using System;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using NetEscapades.Configuration.Yaml;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Logging;
using Rocket.Surgery.Hosting;
using ConfigurationBuilder = Rocket.Surgery.Extensions.Configuration.ConfigurationBuilder;
using IConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting
{
    class RocketContext
    {
        private readonly IHostBuilder _hostBuilder;
        private string[] _args;
        private ICommandLineExecutor _exec;

        public RocketContext(IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
        }

        public void ConfigureCli(IConfigurationBuilder configurationBuilder)
        {
            var rocketHostBuilder = RocketHostExtensions.GetConventionalHostBuilder(_hostBuilder);
            var clb = new CommandLineBuilder(
                rocketHostBuilder.Scanner,
                rocketHostBuilder.AssemblyProvider,
                rocketHostBuilder.AssemblyCandidateFinder,
                rocketHostBuilder.DiagnosticSource,
                rocketHostBuilder.Properties
            );

            _exec = clb.Build().Parse(_args ?? Array.Empty<string>());
            _args = _exec.ApplicationState.RemainingArguments ?? Array.Empty<string>();
            configurationBuilder.AddApplicationState(_exec.ApplicationState);
            rocketHostBuilder.Properties.Add(typeof(ICommandLineExecutor), _exec);
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

        public void ReplaceArguments(IConfigurationBuilder configurationBuilder)
        {
            var commandLineSource = configurationBuilder.Sources.OfType<CommandLineConfigurationSource>()
                .FirstOrDefault();
            if (commandLineSource != null)
            {
                commandLineSource.Args = _args;
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
                context.HostingEnvironment.Convert(),
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
#if !(NETSTANDARD2_0 || NETCOREAPP2_1)
            services.AddHealthChecks();
#endif
        }

        public void ConfigureLogging(HostBuilderContext context, IServiceCollection services)
        {
            var rocketHostBuilder = RocketHostExtensions.GetConventionalHostBuilder(_hostBuilder);
            rocketHostBuilder.UseLogging(new RocketLoggingOptions());
        }

        public void DefaultServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
        {
            var conventionalBuilder = RocketHostExtensions.GetConventionalHostBuilder(_hostBuilder);
            builder.UseServiceProviderFactory(
                new ServicesBuilderServiceProviderFactory(collection =>
                    new ServicesBuilder(
                        conventionalBuilder.Scanner,
                        conventionalBuilder.AssemblyProvider,
                        conventionalBuilder.AssemblyCandidateFinder,
                        collection,
                        context.Configuration,
                        context.HostingEnvironment.Convert(),
                        conventionalBuilder.DiagnosticSource,
                        conventionalBuilder.Properties
                    )
                )
            );
        }
    }
}
