using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
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
        protected DiagnosticLogger Logger { get; }
        protected DiagnosticSource DiagnosticSource { get; }

        protected RocketStartup(
            IConfiguration configuration,
            IHostingEnvironment environment,
            DiagnosticSource diagnosticSource)
        {
            Environment = environment;
            Configuration = configuration;
            DiagnosticSource = diagnosticSource;
            Logger = new DiagnosticLogger(DiagnosticSource);
        }

        public IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public IServiceProvider ApplicationServices { get; private set; }

        protected abstract IServiceProvider ComposeServices(IServiceCollection services);

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            foreach (var d in services
                .Where(x =>
                    x.ServiceType == typeof(IRocketWebHostBuilder) ||
                    x.ServiceType == typeof(IWebHostBuilder) ||
                    x.ServiceType == typeof(IRocketHostBuilder) ||
                    x.ServiceType == typeof(IHostBuilder))
                .ToArray())
            {
                services.Remove(d);
            }

            return ApplicationServices = ComposeServices(services);
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
                    action(this, ApplicationServices, builder);
                }
            }
        }
    }
}
