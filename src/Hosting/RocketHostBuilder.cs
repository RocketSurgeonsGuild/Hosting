using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

namespace Rocket.Surgery.Hosting
{
    class RocketHostBuilder : ConventionHostBuilder, IRocketHostBuilder
    {
        public RocketHostBuilder(IHostBuilder builder, IConventionScanner scanner, IAssemblyCandidateFinder assemblyCandidateFinder, IAssemblyProvider assemblyProvider, DiagnosticSource diagnosticSource, IDictionary<object, object> properties) : base(scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, properties)
        {
            Builder = builder;
        }
        public RocketHostBuilder(IHostBuilder builder, IConventionScanner scanner, IAssemblyCandidateFinder assemblyCandidateFinder, IAssemblyProvider assemblyProvider, DiagnosticSource diagnosticSource, IDictionary<object, object> properties, string[] args) : base(scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, properties)
        {
            Builder = builder;
            Arguments = args;
        }

        public IHostBuilder Builder { get; }

        public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate) => Builder.ConfigureContainer(configureDelegate);
        public IHost Build() => Builder.Build();
        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) => Builder.ConfigureHostConfiguration(configureDelegate);
        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) => Builder.ConfigureAppConfiguration(configureDelegate);
        public IHostBuilder ConfigureServices(Action<IServiceCollection> configureServices) => Builder.ConfigureServices(configureServices);
        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureServices) => Builder.ConfigureServices(configureServices);
        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) => Builder.UseServiceProviderFactory(factory);
        public string[] Arguments { get; set; }

        internal RocketHostBuilder With(IConventionScanner scanner)
        {
            return new RocketHostBuilder(Builder, scanner, AssemblyCandidateFinder, AssemblyProvider, DiagnosticSource, Properties, Arguments);
        }

        internal RocketHostBuilder With(IAssemblyCandidateFinder assemblyCandidateFinder)
        {
            return new RocketHostBuilder(Builder, Scanner, assemblyCandidateFinder, AssemblyProvider, DiagnosticSource, Properties, Arguments);
        }

        internal RocketHostBuilder With(IAssemblyProvider assemblyProvider)
        {
            return new RocketHostBuilder(Builder, Scanner, AssemblyCandidateFinder, assemblyProvider, DiagnosticSource, Properties, Arguments);
        }

        internal RocketHostBuilder With(DiagnosticSource diagnosticSource)
        {
            return new RocketHostBuilder(Builder, Scanner, AssemblyCandidateFinder, AssemblyProvider, diagnosticSource, Properties, Arguments);
        }
    }

    // To be replaced by Host in 3.x
    public static class RocketHost
    {
        public static IHostBuilder CreateDefaultBuilder(string[] args = null)
        {
            var builder = new HostBuilder();

            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder.ConfigureHostConfiguration(config =>
            {
                config.AddEnvironmentVariables(); // TODO: Prefix?
                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            });

            builder.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                    {
                        var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                        if (appAssembly != null)
                        {
                            config.AddUserSecrets(appAssembly, optional: true);
                        }
                    }

                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                });

            return builder;
        }
    }
}
