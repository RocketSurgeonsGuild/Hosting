using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Hosting;
using Rocket.Surgery.Reflection.Extensions;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public abstract class RocketApplicationStartup : IStartup
    {
        private readonly IRocketApplicationServiceComposer _serviceComposer;
        private readonly IDictionary<object, object> _properties;
        private IServiceProvider _services;

        protected RocketApplicationStartup(
            IConventionScanner scanner,
            IRocketApplicationServiceComposer serviceComposer,
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
            if (this is IConvention convention)
            {
                scanner.AppendConvention(convention);
            }
        }

        protected IHostingEnvironment Environment { get; }
        protected IConfiguration Configuration { get; }
        protected DiagnosticLogger Logger { get; }
        protected DiagnosticSource DiagnosticSource { get; }

        protected IServiceProvider ApplicationServices { get; private set; }
        protected IServiceProvider SystemServices { get; private set; }

        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.RemoveAll(typeof(IDictionary<object, object>));
            using (Logger.TimeTrace("{Step}", nameof(ConfigureServices)))
            {
                services.RemoveAll(typeof(AutoRequestServicesStartupFilter));

                _serviceComposer.ComposeServices(services, _properties, out var systemServiceProvider, out var applicationServiceProvider);

                ApplicationServices = applicationServiceProvider;
                SystemServices = systemServiceProvider;

                return applicationServiceProvider;
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            using (Logger.TimeTrace("{Step}", nameof(Configure)))
            {
                var application = new RocketApplicationBuilder(app, Configuration);

                if (GetType().GetMethod("ComposeSystem", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
                {
                    Action<object, IServiceProvider, RocketSystemBuilder> systemAction;
                    using (Logger.TimeTrace("Configuring ComposeSystem Method"))
                    {
                        systemAction = InjectableMethodBuilder.Create(GetType(), "ComposeSystem")
                            .WithParameter<RocketSystemBuilder>()
                            .Compile();
                    }

                    using (Logger.TimeTrace("Invoking ComposeSystem Method"))
                    {
                        application.Map("/system", a =>
                        {
                            var system = new RocketSystemBuilder(a, Configuration);
                            system.UseMiddleware<RequestServicesContainerMiddleware>(
                                SystemServices.GetRequiredService<IServiceScopeFactory>());
                            systemAction(this, SystemServices, system);
                        });
                    }
                }

                Action<object, IServiceProvider, RocketApplicationBuilder> action;
                using (Logger.TimeTrace("Configuring Compose Method"))
                {
                    action = InjectableMethodBuilder.Create(GetType(), "Compose")
                        .WithParameter<RocketApplicationBuilder>()
                        .Compile();
                }

                using (Logger.TimeDebug("Invoking Compose Method"))
                {
                    application.UseMiddleware<RequestServicesContainerMiddleware>(
                        ApplicationServices.GetRequiredService<IServiceScopeFactory>());
                    action(this, ApplicationServices, application);
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
