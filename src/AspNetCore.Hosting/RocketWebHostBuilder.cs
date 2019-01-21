using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    class RocketWebHostBuilder : ConventionHostBuilder, IRocketWebHostBuilder
    {
        public RocketWebHostBuilder(
            IWebHostBuilder builder, 
            IConventionScanner scanner, 
            IAssemblyCandidateFinder assemblyCandidateFinder, 
            IAssemblyProvider assemblyProvider, 
            DiagnosticSource diagnosticSource, 
            IDictionary<object, object> properties) : base(scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, properties)
        {
            Builder = builder;
        }
        internal RocketWebHostBuilder(
            IWebHostBuilder builder, 
            IConventionScanner scanner, 
            IAssemblyCandidateFinder assemblyCandidateFinder, 
            IAssemblyProvider assemblyProvider, 
            DiagnosticSource diagnosticSource, 
            IDictionary<object, object> properties, 
            Func<RocketWebHostBuilder, IConfiguration, Microsoft.Extensions.Hosting.IHostingEnvironment, IRocketServiceComposer> applicationServicesComposeDelegate, 
            Func<RocketWebHostBuilder, IConfiguration, Microsoft.Extensions.Hosting.IHostingEnvironment, IRocketServiceComposer> applicationAndSystemServicesComposeDelegate) : base(scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, properties)
        {
            Builder = builder;
            ApplicationServicesComposeDelegate = applicationServicesComposeDelegate;
            ApplicationAndSystemServicesComposeDelegate = applicationAndSystemServicesComposeDelegate;
        }

        public IWebHostBuilder Builder { get; }

        internal RocketWebHostBuilder With(IConventionScanner scanner)
        {
            return new RocketWebHostBuilder(Builder, scanner, AssemblyCandidateFinder, AssemblyProvider, DiagnosticSource, Properties, ApplicationServicesComposeDelegate, ApplicationAndSystemServicesComposeDelegate);
        }

        internal RocketWebHostBuilder With(IAssemblyCandidateFinder assemblyCandidateFinder)
        {
            return new RocketWebHostBuilder(Builder, Scanner, assemblyCandidateFinder, AssemblyProvider, DiagnosticSource, Properties, ApplicationServicesComposeDelegate, ApplicationAndSystemServicesComposeDelegate);
        }

        internal RocketWebHostBuilder With(IAssemblyProvider assemblyProvider)
        {
            return new RocketWebHostBuilder(Builder, Scanner, AssemblyCandidateFinder, assemblyProvider, DiagnosticSource, Properties, ApplicationServicesComposeDelegate, ApplicationAndSystemServicesComposeDelegate);
        }

        internal RocketWebHostBuilder With(DiagnosticSource diagnosticSource)
        {
            return new RocketWebHostBuilder(Builder, Scanner, AssemblyCandidateFinder, AssemblyProvider, diagnosticSource, Properties, ApplicationServicesComposeDelegate, ApplicationAndSystemServicesComposeDelegate);
        }

        internal RocketWebHostBuilder With(IDictionary<object, object> properties)
        {
            return new RocketWebHostBuilder(Builder, Scanner, AssemblyCandidateFinder, AssemblyProvider, DiagnosticSource, properties, ApplicationServicesComposeDelegate, ApplicationAndSystemServicesComposeDelegate);
        }

        internal Func<RocketWebHostBuilder, IConfiguration, Microsoft.Extensions.Hosting.IHostingEnvironment, IRocketServiceComposer> ApplicationServicesComposeDelegate { get; set; }
        internal Func<RocketWebHostBuilder, IConfiguration, Microsoft.Extensions.Hosting.IHostingEnvironment, IRocketServiceComposer> ApplicationAndSystemServicesComposeDelegate { get; set; }
        IWebHost IWebHostBuilder.Build() => Builder.Build();

        IWebHostBuilder IWebHostBuilder.ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate) => Builder.ConfigureAppConfiguration(configureDelegate);

        IWebHostBuilder IWebHostBuilder.ConfigureServices(Action<IServiceCollection> configureServices) => Builder.ConfigureServices(configureServices);

        IWebHostBuilder IWebHostBuilder.ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices) => Builder.ConfigureServices(configureServices);

        string IWebHostBuilder.GetSetting(string key) => Builder.GetSetting(key);

        IWebHostBuilder IWebHostBuilder.UseSetting(string key, string value) => Builder.UseSetting(key, value);
    }
}
