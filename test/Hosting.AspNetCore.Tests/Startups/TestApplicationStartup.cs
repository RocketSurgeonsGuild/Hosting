using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.AspNetCore.Hosting;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests.Startups
{
    class TestApplicationStartup : RocketStartup
    {
        public TestApplicationStartup(
            IRocketWebHostingContext context,
            IRocketServiceComposer serviceComposer,
            IConfiguration configuration,
            IHostEnvironmentvironment) : base(context, serviceComposer, configuration, environment)
        {
        }

        public void ComposeSystem(IApplicationBuilder app)
        {
            app.Use((context, func) => context.Response.WriteAsync("TestApplicationStartup -> ComposeSystem"));
        }

        public void Compose(IApplicationBuilder app)
        {
            app.Use((context, func) => context.Response.WriteAsync("TestApplicationStartup -> Compose"));
        }
    }
}
