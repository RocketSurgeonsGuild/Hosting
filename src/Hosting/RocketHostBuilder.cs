using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Builders;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using ConfigurationBuilder = Rocket.Surgery.Extensions.Configuration.ConfigurationBuilder;
using IMsftConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;
using MsftConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace Rocket.Surgery.Hosting
{
    public class RocketHostBuilder : Builder, IRocketHostBuilder
    {
        private readonly List<Action<IMsftConfigurationBuilder>> _configureHostConfigActions = new List<Action<IMsftConfigurationBuilder>>();
        private readonly IHostBuilder _hostBuilder;
        private readonly string[] _arguments;
        private ServicesBuilderDelegate _servicesBuilderDelegate;

        public RocketHostBuilder(
            IHostBuilder hostBuilder,
            IConventionScanner scanner,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            IAssemblyProvider assemblyProvider,
            DiagnosticSource diagnosticSource,
            string[] arguments = null)
            : base(hostBuilder.Properties)
        {
            _hostBuilder = hostBuilder;
            Scanner = scanner;
            AssemblyCandidateFinder = assemblyCandidateFinder;
            AssemblyProvider = assemblyProvider;
            DiagnosticSource = diagnosticSource;
            _arguments = arguments;
            _servicesBuilderDelegate = (conventionScanner, provider, finder, services, configuration, environment, logger1, properties) =>
                new ServicesBuilder(Scanner, AssemblyProvider, AssemblyCandidateFinder, services, configuration, environment, diagnosticSource, Properties);

            _hostBuilder.ConfigureServices(ConfigureDefaultServices);
            _hostBuilder.ConfigureAppConfiguration(DefaultApplicationConfiguration);
            ((IRocketHostBuilder)this).PrependConvention(new StandardConfigurationConvention());
            UseCli = _arguments != null;
        }

        private void DefaultApplicationConfiguration(HostBuilderContext context, IMsftConfigurationBuilder configurationBuilder)
        {
            // remove standard configurations
            configurationBuilder.Sources.Clear();
            var cb = new ConfigurationBuilder(
                Scanner,
                (IHostingEnvironment)context.HostingEnvironment,
                context.Configuration,
                configurationBuilder,
                DiagnosticSource,
                Properties);
            cb.Build();
        }

        private void ConfigureDefaultServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton(Scanner);
            services.AddSingleton(AssemblyProvider);
            services.AddSingleton(AssemblyCandidateFinder);
        }

        public IConventionScanner Scanner { get; }
        public IAssemblyCandidateFinder AssemblyCandidateFinder { get; }
        public IAssemblyProvider AssemblyProvider { get; }
        public DiagnosticSource DiagnosticSource { get; }
        public bool UseCli { get; set; }

        public IHost Build()
        {
            if (UseCli)
            {
                var clb = new CommandLineBuilder(
                    Scanner,
                    AssemblyProvider,
                    AssemblyCandidateFinder,
                    DiagnosticSource,
                    Properties
                );
                clb.OnParse(state =>
                {
                    this.ConfigureServices(services =>
                    {
                        services.AddSingleton(state);
                    });

                    Properties[typeof(IApplicationState)] = state;
                    ((IRocketHostBuilder)this).AppendConvention(new CliConfigurationConvention());
                    ((IRocketHostBuilder)this).AppendConvention(new FinalConfigurationConvention(state.RemainingArguments));
                });
                var executor = clb.Build().Parse(_arguments ?? Array.Empty<string>());
                this.ConfigureServices(services =>
                {
                    services.AddSingleton(executor);
                    services.AddSingleton<IHostLifetime, CliLifetime>();
                });
            }
            else
            {
                ((IRocketHostBuilder)this).AppendConvention(new FinalConfigurationConvention());
            }

            IConfiguration appConfiguration = null;
            IHostingEnvironment hostingEnvironment = null;
            _hostBuilder.ConfigureServices((context, services) =>
            {
                appConfiguration = context.Configuration;
                hostingEnvironment = context.HostingEnvironment;
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

        public IRocketHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IMsftConfigurationBuilder> configureDelegate)
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

        public IRocketHostBuilder ConfigureHostConfiguration(Action<IMsftConfigurationBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureHostConfiguration(configureDelegate);
            return this;
        }

        public IRocketHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
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

        public IRocketHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureContainer(configureDelegate);
            return this;
        }

        public IRocketHostBuilder UseServicesBuilderFactory(ServicesBuilderDelegate configureDelegate)
        {
            _servicesBuilderDelegate = configureDelegate;
            return this;
        }

        public IRocketHostBuilder AppendConvention(IConvention convention)
        {
            Scanner.AppendConvention(convention ?? throw new ArgumentNullException(nameof(convention)));
            return this;
        }

        public IRocketHostBuilder AppendDelegate(Delegate @delegate)
        {
            Scanner.AppendDelegate(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        public IRocketHostBuilder ExceptConvention(Type type)
        {
            Scanner.ExceptConvention(type ?? throw new ArgumentNullException(nameof(type)));
            return this;
        }

        public IRocketHostBuilder ExceptConvention(Assembly assembly)
        {
            Scanner.ExceptConvention(assembly ?? throw new ArgumentNullException(nameof(assembly)));
            return this;
        }

        public IRocketHostBuilder PrependConvention(IConvention convention)
        {
            Scanner.PrependConvention(convention ?? throw new ArgumentNullException(nameof(convention)));
            return this;
        }

        public IRocketHostBuilder PrependDelegate(Delegate @delegate)
        {
            Scanner.PrependDelegate(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        public IHostBuilder AsHostBuilder() => this;

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
