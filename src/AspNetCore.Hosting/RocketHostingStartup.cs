using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using NetEscapades.Configuration.Yaml;
using Rocket.Surgery.AspNetCore.Hosting;
using ConfigurationBuilder = Rocket.Surgery.Extensions.Configuration.ConfigurationBuilder;

[assembly: HostingStartup(typeof(RocketHostingStartup))]
namespace Rocket.Surgery.AspNetCore.Hosting
{
    public class RocketHostingStartup : IHostingStartup
    {
        private RocketWebHostBuilder _rocketWebHostBuilder;
        public void Configure(IWebHostBuilder builder)
        {
            IRocketWebHostBuilder conventionalBuilder = _rocketWebHostBuilder = RocketWebHostExtensions.GetConventionalWebHostBuilder(builder);

            conventionalBuilder.ConfigureServices(ConfigureDefaultServices);
            conventionalBuilder.ConfigureAppConfiguration(DefaultApplicationConfiguration);
        }

        private void DefaultApplicationConfiguration(WebHostBuilderContext context, IConfigurationBuilder configurationBuilder)
        {
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
                _rocketWebHostBuilder.Scanner,
                context.HostingEnvironment as Microsoft.Extensions.Hosting.IHostingEnvironment,
                context.Configuration,
                configurationBuilder,
                _rocketWebHostBuilder.DiagnosticSource,
                _rocketWebHostBuilder.Properties);
            cb.Build();

            MoveConfigurationSourceToEnd(configurationBuilder.Sources,
                sources => sources.OfType<JsonConfigurationSource>().Where(x =>
                    string.Equals(x.Path, "secrets.json", StringComparison.OrdinalIgnoreCase)));
            MoveConfigurationSourceToEnd(configurationBuilder.Sources,
                sources => sources.OfType<EnvironmentVariablesConfigurationSource>());
            MoveConfigurationSourceToEnd(configurationBuilder.Sources,
                sources => sources.OfType<CommandLineConfigurationSource>());
        }

        private void InsertConfigurationSourceAfter<T>(IList<IConfigurationSource> sources, Func<IList<IConfigurationSource>, T> getSource, Func<T, IConfigurationSource> createSourceFrom)
            where T : IConfigurationSource
        {
            var source = getSource(sources);
            if (source != null)
            {
                var index = sources.IndexOf(source);
                sources.Insert(index + 1, createSourceFrom(source));
            }
        }

        private void MoveConfigurationSourceToEnd<T>(IList<IConfigurationSource> sources, Func<IList<IConfigurationSource>, IEnumerable<T>> getSource)
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

        private void ConfigureDefaultServices(WebHostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IRocketWebHostingContext>(_ => new RocketWebHostingContext(_rocketWebHostBuilder));
        }
    }
}
