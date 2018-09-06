using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.AspNetCore.Hosting.Cli;
using Rocket.Surgery.Builders;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Hosting;
using ConfigurationBuilder = Rocket.Surgery.Extensions.Configuration.ConfigurationBuilder;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;
using IWebHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using IMsftConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;
using MsftConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public partial class RocketWebHostBuilder : Builder, IRocketWebHostBuilder
    {
        private readonly IWebHostBuilder _webHostBuilder;
        private readonly WebHostBuilderContext _context;
        private readonly string[] _arguments;

        private readonly FieldInfo _contextProperty = typeof(WebHostBuilder)
            .GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);

        public RocketWebHostBuilder(
            IWebHostBuilder webHostBuilder,
            IConventionScanner scanner,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            IAssemblyProvider assemblyProvider,
            DiagnosticSource diagnosticSource,
            string[] arguments = null) : base(new Dictionary<object, object>())
        {
            _webHostBuilder = webHostBuilder;
            _context = (WebHostBuilderContext)_contextProperty.GetValue(webHostBuilder);
            Scanner = scanner;
            AssemblyCandidateFinder = assemblyCandidateFinder;
            AssemblyProvider = assemblyProvider;
            DiagnosticSource = diagnosticSource;
            _arguments = arguments;
            _webHostBuilder.ConfigureServices(ConfigureDefaultServices);

            _webHostBuilder.ConfigureAppConfiguration(DefaultApplicationConfiguration);
            ((IRocketWebHostBuilder)this).PrependConvention(new StandardConfigurationConvention());
            UseCli = _arguments != null;
        }

        private void DefaultApplicationConfiguration(WebHostBuilderContext context, IMsftConfigurationBuilder configurationBuilder)
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

        private void ConfigureDefaultServices(WebHostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton(Scanner);
            services.AddSingleton(AssemblyProvider);
            services.AddSingleton(AssemblyCandidateFinder);
            services.AddSingleton(Properties);
            services.AddSingleton<IRocketServiceComposer, RocketServiceComposer>();
            services.AddSingleton<IRocketApplicationServiceComposer, RocketApplicationServiceComposer>();
        }

        public IConventionScanner Scanner { get; }
        public IAssemblyCandidateFinder AssemblyCandidateFinder { get; }
        public IAssemblyProvider AssemblyProvider { get; }
        public DiagnosticSource DiagnosticSource { get; }
        public bool UseCli { get; set; }

        public IWebHost Build()
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
                    if (!state.IsDefaultCommand)
                    {
                        this.UseServer(new CliServer());
                    }
                    _webHostBuilder.ConfigureServices(services =>
                    {
                        services.AddSingleton(state);
                    });
                    Properties[typeof(IApplicationState)] = state;

                    ((IRocketWebHostBuilder)this).AppendConvention(new FinalConfigurationConvention(state.RemainingArguments));
                });
                var executor = clb.Build().Parse(_arguments ?? Array.Empty<string>());
                _webHostBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton(executor);
                });
            }
            else
            {
                ((IRocketWebHostBuilder)this).AppendConvention(new FinalConfigurationConvention());
            }

            return _webHostBuilder.Build();
        }
    }
}
