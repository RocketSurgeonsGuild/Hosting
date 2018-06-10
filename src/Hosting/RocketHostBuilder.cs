using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Builders;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using ConfigurationBuilder = Rocket.Surgery.Extensions.Configuration.ConfigurationBuilder;
using IMsftConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;
using MsftConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace Rocket.Surgery.Hosting
{
    public class RocketHostBuilder : Builder, IRocketHostBuilder
    {
        private readonly IHostBuilder _hostBuilder;
        private ServicesBuilderDelegate _servicesBuilderDelegate;

        public RocketHostBuilder(
            IHostBuilder hostBuilder,
            IConventionScanner scanner,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            IAssemblyProvider assemblyProvider,
            DiagnosticSource diagnosticSource)
            : base(hostBuilder.Properties)
        {
            _hostBuilder = hostBuilder;
            Scanner = scanner;
            AssemblyCandidateFinder = assemblyCandidateFinder;
            AssemblyProvider = assemblyProvider;
            DiagnosticSource = diagnosticSource;
            _servicesBuilderDelegate = (conventionScanner, provider, finder, services, configuration, environment, logger1, properties) =>
                new ServicesBuilder(Scanner, AssemblyProvider, AssemblyCandidateFinder, services, configuration, environment, diagnosticSource, Properties);
        }

        public IConventionScanner Scanner { get; }
        public IAssemblyCandidateFinder AssemblyCandidateFinder { get; }
        public IAssemblyProvider AssemblyProvider { get; }
        public DiagnosticSource DiagnosticSource { get; }

        public IHost Build()
        {
            IConfiguration appConfiguration = null;
            IHostingEnvironment hostingEnvironment = null;
            _hostBuilder.ConfigureServices((context, services) =>
            {
                appConfiguration = context.Configuration;
                hostingEnvironment = context.HostingEnvironment;
                services.AddSingleton(Scanner);
                services.AddSingleton(AssemblyProvider);
                services.AddSingleton(AssemblyCandidateFinder);
                services.AddSingleton<IRocketHostBuilder>(this);
                services.AddSingleton<IHostBuilder>(this);
            });

            _hostBuilder.UseServiceProviderFactory(
                new ServicesBuilderServiceProviderFactory(collection =>
                    _servicesBuilderDelegate(
                        Scanner,
                        AssemblyProvider,
                        AssemblyCandidateFinder,
                        collection,
                        appConfiguration,
                        hostingEnvironment,
                        DiagnosticSource,
                        Properties
                    )
                )
            );

            _hostBuilder.ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                var cb = new ConfigurationBuilder(
                    Scanner,
                    context.HostingEnvironment,
                    context.Configuration,
                    configurationBuilder,
                    DiagnosticSource,
                    Properties);
                cb.Build();
            });

            return _hostBuilder.Build();
        }

        IRocketHostBuilder IRocketHostBuilder.ConfigureAppConfiguration(Action<HostBuilderContext, IMsftConfigurationBuilder> configureDelegate)
        {
            if (configureDelegate == null)
            {
                throw new ArgumentNullException(nameof(configureDelegate));
            }

            Scanner.AppendDelegate(new ConfigurationConventionDelegate(context =>
            {
                configureDelegate(new HostBuilderContext(context.Properties)
                {
                    HostingEnvironment = context.Environment,
                    Configuration = context.Configuration
                }, context);
            }));
            return this;
        }

        IRocketHostBuilder IRocketHostBuilder.ConfigureHostConfiguration(Action<IMsftConfigurationBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureHostConfiguration(configureDelegate);
            return this;
        }

        IRocketHostBuilder IRocketHostBuilder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if (configureDelegate == null)
            {
                throw new ArgumentNullException(nameof(configureDelegate));
            }

            Scanner.AppendDelegate(new ServiceConventionDelegate(context =>
            {
                configureDelegate(new HostBuilderContext(context.Properties)
                {
                    HostingEnvironment = context.Environment,
                    Configuration = context.Configuration
                }, context.Services);
            }));
            return this;
        }

        IRocketHostBuilder IRocketHostBuilder.ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureContainer(configureDelegate);
            return this;
        }

        IRocketHostBuilder IRocketHostBuilder.UseServicesBuilderFactory(ServicesBuilderDelegate configureDelegate)
        {
            _servicesBuilderDelegate = configureDelegate;
            return this;
        }

        IRocketHostBuilder IRocketSurgeryHostBuilder<IRocketHostBuilder>.AppendConvention(IConvention convention)
        {
            Scanner.AppendConvention(convention ?? throw new ArgumentNullException(nameof(convention)));
            return this;
        }

        IRocketHostBuilder IRocketSurgeryHostBuilder<IRocketHostBuilder>.AppendDelegate(Delegate @delegate)
        {
            Scanner.AppendDelegate(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        IRocketHostBuilder IRocketSurgeryHostBuilder<IRocketHostBuilder>.ExceptConvention(Type type)
        {
            Scanner.ExceptConvention(type ?? throw new ArgumentNullException(nameof(type)));
            return this;
        }

        IRocketHostBuilder IRocketSurgeryHostBuilder<IRocketHostBuilder>.ExceptConvention(Assembly assembly)
        {
            Scanner.ExceptConvention(assembly ?? throw new ArgumentNullException(nameof(assembly)));
            return this;
        }

        IRocketHostBuilder IRocketSurgeryHostBuilder<IRocketHostBuilder>.PrependConvention(IConvention convention)
        {
            Scanner.PrependConvention(convention ?? throw new ArgumentNullException(nameof(convention)));
            return this;
        }

        IRocketHostBuilder IRocketSurgeryHostBuilder<IRocketHostBuilder>.PrependDelegate(Delegate @delegate)
        {
            Scanner.PrependDelegate(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        IHostBuilder IRocketHostBuilder.AsHostBuilder() => this;

        IHostBuilder IHostBuilder.ConfigureHostConfiguration(Action<IMsftConfigurationBuilder> configureDelegate)
        {
            ((IRocketHostBuilder)this).ConfigureHostConfiguration(configureDelegate);
            return this;
        }

        IHostBuilder IHostBuilder.ConfigureAppConfiguration(Action<HostBuilderContext, IMsftConfigurationBuilder> configureDelegate)
        {
            ((IRocketHostBuilder)this).ConfigureAppConfiguration(configureDelegate);
            return this;
        }

        IHostBuilder IHostBuilder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            ((IRocketHostBuilder)this).ConfigureServices(configureDelegate);
            return this;
        }

        IHostBuilder IHostBuilder.ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            ((IRocketHostBuilder)this).ConfigureContainer(configureDelegate);
            return this;
        }

        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            // Yes this breaks SOLID :(
            throw new NotSupportedException("UseServiceProviderFactory cannot be used with RocketHostBuilder");
        }
    }
}
