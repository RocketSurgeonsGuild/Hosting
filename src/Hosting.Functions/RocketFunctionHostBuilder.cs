using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.WebJobs;
using ConfigurationBuilder = Rocket.Surgery.Extensions.Configuration.ConfigurationBuilder;
using MsftConfigurationBinder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace Rocket.Surgery.Hosting.Functions
{
    class RocketFunctionHostBuilder : ConventionHostBuilder<IRocketFunctionHostBuilder>, IRocketFunctionHostBuilder
    {
        private readonly object _startupInstance;
        private readonly IRocketEnvironment _environment;

        public RocketFunctionHostBuilder(
            IWebJobsBuilder builder,
            Assembly functionsAssembly,
            object startupInstance,
            IRocketEnvironment environment,
            IConventionScanner scanner,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            IAssemblyProvider assemblyProvider,
            DiagnosticSource diagnosticSource) : base(scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, new ServiceProviderDictionary())
        {
            _startupInstance = startupInstance;
            _environment = environment ?? CreateEnvironment();
            Builder = builder;
            FunctionsAssembly = functionsAssembly;
        }

        public IWebJobsBuilder Builder { get; }
        public Assembly FunctionsAssembly { get; }

        internal RocketFunctionHostBuilder With(IConventionScanner scanner)
        {
            return new RocketFunctionHostBuilder(Builder, FunctionsAssembly, _startupInstance, _environment, scanner, AssemblyCandidateFinder, AssemblyProvider, DiagnosticSource);
        }

        internal RocketFunctionHostBuilder With(Assembly assemby)
        {
            return new RocketFunctionHostBuilder(Builder, assemby, _startupInstance, _environment, Scanner, AssemblyCandidateFinder, AssemblyProvider, DiagnosticSource);
        }

        internal RocketFunctionHostBuilder With(IAssemblyCandidateFinder assemblyCandidateFinder)
        {
            return new RocketFunctionHostBuilder(Builder, FunctionsAssembly, _startupInstance, _environment, Scanner, assemblyCandidateFinder, AssemblyProvider, DiagnosticSource);
        }

        internal RocketFunctionHostBuilder With(IAssemblyProvider assemblyProvider)
        {
            return new RocketFunctionHostBuilder(Builder, FunctionsAssembly, _startupInstance, _environment, Scanner, AssemblyCandidateFinder, assemblyProvider, DiagnosticSource);
        }

        internal RocketFunctionHostBuilder With(DiagnosticSource diagnosticSource)
        {
            return new RocketFunctionHostBuilder(Builder, FunctionsAssembly, _startupInstance, _environment, Scanner, AssemblyCandidateFinder, AssemblyProvider, diagnosticSource);
        }

        internal RocketFunctionHostBuilder With(IRocketEnvironment environment)
        {
            return new RocketFunctionHostBuilder(Builder, FunctionsAssembly, _startupInstance, environment, Scanner, AssemblyCandidateFinder, AssemblyProvider, DiagnosticSource);
        }

        private static IRocketEnvironment CreateEnvironment()
        {
            var environmentNames = new[]
            {
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                Environment.GetEnvironmentVariable("WEBSITE_SLOT_NAME"),
                "Unknown"
            };

            var applicationNames = new[]
            {
                Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"),
                "Functions"
            };

            var environment = new RocketEnvironment(
                environmentNames.First(x => !string.IsNullOrEmpty(x)),
                applicationNames.First(x => !string.IsNullOrEmpty(x)),
                contentRootPath: null,
                contentRootFileProvider: null
            );

            return environment;
        }

        private IConfiguration SetupConfiguration()
        {
            var existingConfiguration = Builder.Services.First(z => z.ServiceType == typeof(IConfiguration))
                .ImplementationInstance as IConfiguration;

            var configurationBuilder = new MsftConfigurationBinder();
            configurationBuilder.AddConfiguration(existingConfiguration);

            configurationBuilder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddYamlFile("appsettings.yml", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddYamlFile($"appsettings.{_environment.EnvironmentName}.yml", optional: true, reloadOnChange: true);

            var cb = new ConfigurationBuilder(
                Scanner,
                _environment,
                existingConfiguration,
                configurationBuilder,
                DiagnosticSource,
                Properties);

            cb.Build();

            if (_environment.IsDevelopment() && !string.IsNullOrEmpty(_environment.ApplicationName))
            {
                var appAssembly = Assembly.Load(new AssemblyName(_environment.ApplicationName));
                if (appAssembly != null)
                {
                    configurationBuilder.AddUserSecrets(appAssembly, optional: true);
                }
            }

            configurationBuilder.AddEnvironmentVariables();

            var newConfig = configurationBuilder.Build();

            Builder.Services.Replace(ServiceDescriptor.Singleton<IConfiguration>(newConfig));
            return newConfig;
        }

        private void SetupServices(IConfiguration existingConfiguration)
        {
            var builder = new ServicesBuilder(
                Scanner,
                AssemblyProvider,
                AssemblyCandidateFinder,
                Builder.Services,
                existingConfiguration,
                _environment,
                DiagnosticSource,
                Properties);

            Composer.Register<IServiceConventionContext, IServiceConvention, ServiceConventionDelegate>(Scanner, builder);
        }

        private void SetupWebJobs(IConfiguration existingConfiguration)
        {
            var builder = new WebJobsConventionBuilder(
                Scanner,
                AssemblyProvider,
                AssemblyCandidateFinder,
                Builder,
                existingConfiguration,
                _environment,
                DiagnosticSource,
                Properties);

            Composer.Register<IWebJobsConventionContext, IWebJobsConvention, WebJobsConventionDelegate>(Scanner, builder);
        }

        public void Compose()
        {
            if (_startupInstance is IConvention convention)
            {
                Scanner.AppendConvention(convention);
            }

            var configuration = SetupConfiguration();
            SetupServices(configuration);
            SetupWebJobs(configuration);
        }
    }
}
