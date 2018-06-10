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
    class TestStartup : RocketStartup
    {
        private readonly IRocketWebHostBuilder _builder;
        public TestStartup(
            IConfiguration configuration,
            IHostingEnvironment environment,
            DiagnosticSource diagnosticSource,
            IRocketWebHostBuilder builder) : base(configuration, environment, diagnosticSource)
        {
            _builder = builder;
        }
        protected override IServiceProvider ComposeServices(IServiceCollection services)
        {
            return new ServicesBuilder(
                _builder.Scanner,
                _builder.AssemblyProvider,
                _builder.AssemblyCandidateFinder,
                services,
                Configuration,
                Environment,
                DiagnosticSource,
                _builder.Properties
            ).Build();
        }

        public void Compose(RocketApplicationBuilder app)
        {
            app.Use((context, func) => context.Response.WriteAsync("TestStartup -> Compose"));
        }
    }
}
