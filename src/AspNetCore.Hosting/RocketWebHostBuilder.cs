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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Builders;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Configuration;
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

        private readonly FieldInfo _contextProperty = typeof(WebHostBuilder)
            .GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);

        public RocketWebHostBuilder(
            IWebHostBuilder webHostBuilder,
            IConventionScanner scanner,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            IAssemblyProvider assemblyProvider,
            DiagnosticSource diagnosticSource) : base(new Dictionary<object, object>())
        {
            _webHostBuilder = webHostBuilder;
            _context = (WebHostBuilderContext)_contextProperty.GetValue(webHostBuilder);
            Scanner = scanner;
            AssemblyCandidateFinder = assemblyCandidateFinder;
            AssemblyProvider = assemblyProvider;
            DiagnosticSource = diagnosticSource;
            _webHostBuilder.ConfigureServices(ConfigureDefaultServices);
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

        public IWebHost Build()
        {
            _webHostBuilder.ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                var cb = new ConfigurationBuilder(
                    Scanner,
                    (IHostingEnvironment)context.HostingEnvironment,
                    context.Configuration,
                    configurationBuilder,
                    DiagnosticSource,
                    Properties);
                cb.Build();
            });

            return _webHostBuilder.Build();
        }
    }
}
