using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Reflection.Extensions;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public abstract class RocketStartup : IStartup
    {
        private readonly IRocketHostingContext _context;
        private readonly IRocketServiceComposer _serviceComposer;
        protected string SystemPath = "/system";

        protected RocketStartup(
            IRocketHostingContext context,
            IRocketServiceComposer serviceComposer,
            IConfiguration configuration,
            IHostingEnvironment environment)
        {
            _context = context;
            _serviceComposer = serviceComposer;
            Environment = environment;
            Configuration = configuration;
            Logger = new DiagnosticLogger(context.DiagnosticSource);
            if (this is IConvention convention)
            {
                context.Scanner.AppendConvention(convention);
            }
        }

        public IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public ILogger Logger { get; }
        public IServiceProvider ApplicationServices { get; private set; }
        public IServiceProvider SystemServices { get; private set; }

        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.RemoveAll(typeof(IDictionary<object, object>));
            using (Logger.TimeTrace("{Step}", nameof(ConfigureServices)))
            {
                services.RemoveAll(typeof(AutoRequestServicesStartupFilter));

                _serviceComposer.ComposeServices(
                    services,
                    _context.Properties,
                    out var systemServiceProvider,
                    out var applicationServiceProvider);

                ApplicationServices = applicationServiceProvider;
                SystemServices = systemServiceProvider;

                return applicationServiceProvider;
            }
        }

        public virtual void Configure(IApplicationBuilder application)
        {
            if (SystemServices != null)
            {
                using (Logger.TimeTrace("{Step}", nameof(Configure)))
                {
                    application.Map(SystemPath, a => ConfigureSystem(new ApplicationBuilder(SystemServices)));
                }
            }

            if (GetType().GetMethod("Compose", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
            {
                using (Logger.TimeTrace("Configuring Compose Method"))
                {
                    var action = InjectableMethodBuilder.Create(GetType(), "Compose")
                        .WithParameter<IApplicationBuilder>()
                        .Compile();

                    using (Logger.TimeDebug("Invoking Compose Method"))
                    {
                        application.UseMiddleware<RequestServicesContainerMiddleware>(
                            ApplicationServices.GetRequiredService<IServiceScopeFactory>());
                        action(this, ApplicationServices, application);
                    }
                }
            }
            else
            {
                Logger.LogTrace("Missing Compose method, you probably didn't meant to get here!");
            }
        }

        public virtual void ConfigureSystem(IApplicationBuilder applicationBuilder)
        {
            if (GetType().GetMethod("ComposeSystem", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
            {
                using (Logger.TimeTrace("Configuring ComposeSystem Method"))
                {
                    var systemAction = InjectableMethodBuilder.Create(GetType(), "ComposeSystem")
                        .WithParameter<IApplicationBuilder>()
                        .Compile();

                    using (Logger.TimeTrace("Invoking ComposeSystem Method"))
                    {
                        applicationBuilder.UseMiddleware<RequestServicesContainerMiddleware>(
                        SystemServices.GetRequiredService<IServiceScopeFactory>());
                        systemAction(this, SystemServices, applicationBuilder);
                    }
                }
            }
            else
            {
                Logger.LogTrace("Missing ComposeSystem method, you might have meant to get here!");
            }
        }
    }
}
