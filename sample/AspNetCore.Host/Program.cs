using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Extensions
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var diagnosticSource = new DiagnosticListener("Extensions.Host");

            var builder = Host.CreateDefaultBuilder()
                .LaunchWith(RocketBooster.For(DependencyContext.Default, diagnosticSource))
                .ConfigureWebHostDefaults(x => x
                    .UseKestrel()
                    .UseStartup<Startup>()
                );
            return builder
                .Build()
                .RunAsync();
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
        }

        public void Compose(IApplicationBuilder app)
        {
            app.Use((context, func) => context.Response.WriteAsync("TestStartup -> Compose"));
        }
    }

    class Convention : IServiceConvention, IConfigurationConvention
    {
        public void Register(IServiceConventionContext context)
        {
            context.Services
                .AddLogging()
                .AddOptions();
        }

        public void Register(IConfigurationConventionContext context)
        {
            context.AddInMemoryCollection(new Dictionary<string, string>()
            {
                ["abcd"] = "1234"
            });
        }
    }
}
