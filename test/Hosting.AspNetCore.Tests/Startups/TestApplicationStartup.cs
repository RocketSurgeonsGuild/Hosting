using System;
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
        private readonly IRocketWebHostBuilder _builder;

        public TestApplicationStartup(
            IConfiguration configuration, 
            IHostingEnvironment environment, 
            DiagnosticSource diagnosticSource,
            IRocketWebHostBuilder builder) : base(configuration, environment, diagnosticSource)
        {
            _builder = builder;
        }

        protected override void ComposeServices(IServiceCollection services, out IServiceProvider systemServiceProvider,
            out IServiceProvider applicationServiceProvider)
        {
            var builder = new ApplicationServicesBuilder(
                _builder.Scanner,
                _builder.AssemblyProvider,
                _builder.AssemblyCandidateFinder,
                services,
                Configuration,
                Environment,
                DiagnosticSource,
                _builder.Properties
            );

            (systemServiceProvider, applicationServiceProvider) = builder.Build();
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
