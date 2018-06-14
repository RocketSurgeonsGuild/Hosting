using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Hosting;
using Rocket.Surgery.Reflection.Extensions;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    // 1. Create factory
    // 2. Create twp base classes (one for system / application, one for just application)
    // 3. Add tests to validate 3 main scenarios.

    public abstract class RocketStartup : IStartup
    {
        private readonly IRocketServiceComposer _serviceComposer;
        private readonly IDictionary<object, object> _properties;
        private IServiceProvider _services;

        protected RocketStartup(
            IRocketServiceComposer serviceComposer,
            IConfiguration configuration,
            IHostingEnvironment environment,
            DiagnosticSource diagnosticSource,
            IDictionary<object, object> properties)
        {
            _serviceComposer = serviceComposer;
            _properties = properties;
            Environment = environment;
            Configuration = configuration;
            DiagnosticSource = diagnosticSource;
            Logger = new DiagnosticLogger(DiagnosticSource);
        }

        protected IHostingEnvironment Environment { get; }
        protected IConfiguration Configuration { get; }
        protected DiagnosticLogger Logger { get; }
        protected DiagnosticSource DiagnosticSource { get; }

        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.RemoveAll(typeof(IDictionary<object, object>));
            using (Logger.TimeTrace("{Step}", nameof(ConfigureServices)))
            {
                return _services = _serviceComposer.ComposeServices(services, _properties);
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            using (Logger.TimeTrace("{Step}", nameof(Configure)))
            {
                var builder = new RocketApplicationBuilder(app, Configuration);

                Action<object, IServiceProvider, RocketApplicationBuilder> action;
                using (Logger.TimeTrace("Configuring Compose Method"))
                {
                    action = InjectableMethodBuilder.Create(GetType(), "Compose")
                        .WithParameter<RocketApplicationBuilder>()
                        .Compile();
                }

                using (Logger.TimeDebug("Invoking Compose Method"))
                {
                    action(this, _services, builder);
                }
            }
        }

        public Action<IApplicationBuilder> App(Action<RocketApplicationBuilder> applicationBuilderAction)
        {
            return (app) =>
            {
                var ab = new RocketApplicationBuilder(app, Configuration);
                applicationBuilderAction(ab);
            };
        }
    }
}
