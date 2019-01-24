using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
}
