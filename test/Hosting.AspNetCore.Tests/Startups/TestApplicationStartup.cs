using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.AspNetCore.Hosting;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests.Startups
{
    class TestApplicationStartup : RocketApplicationStartup
    {
        public TestApplicationStartup(
            IRocketApplicationServiceComposer serviceComposer,
            IConfiguration configuration, 
            IHostingEnvironment environment, 
            DiagnosticSource diagnosticSource,
            IDictionary<object, object> properties) : base(serviceComposer, configuration, environment, diagnosticSource, properties)
        {
        }

        public void ComposeSystem(RocketSystemBuilder app)
        {
            app.Use((context, func) => context.Response.WriteAsync("TestApplicationStartup -> ComposeSystem"));
        }

        public void Compose(RocketApplicationBuilder app)
        {
            app.Use((context, func) => context.Response.WriteAsync("TestApplicationStartup -> Compose"));
        }
    }
}
