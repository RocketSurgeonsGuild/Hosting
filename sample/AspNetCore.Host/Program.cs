using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.AspNetCore.Hosting;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Extensions.Host
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(DependencyContext.Default);
            var assemblyProvider = new DependencyContextAssemblyProvider(DependencyContext.Default);
            var diagnosticSource = new DiagnosticListener("Extensions.Host");
            var builder = new RocketWebHostBuilder(new WebHostBuilder(), new AggregateConventionScanner(assemblyCandidateFinder),
                assemblyCandidateFinder, assemblyProvider, diagnosticSource, args);
            builder
                .UseKestrel()
                .UseStartup<Startup>();
            return builder
                .GoAsync();
        }
    }

    public class Startup : RocketStartup
    {
        public Startup(IConventionScanner scanner, IRocketServiceComposer serviceComposer, IConfiguration configuration, IHostingEnvironment environment, DiagnosticSource diagnosticSource, IDictionary<object, object> properties) : base(scanner, serviceComposer, configuration, environment, diagnosticSource, properties)
        {
        }

        public void Compose(RocketApplicationBuilder app)
        {
            app.Use((context, func) => context.Response.WriteAsync("TestStartup -> Compose"));
        }
    }

    [Command]
    class MyCommand
    {
        private readonly IConfiguration _configuration;

        public MyCommand(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Option(CommandOptionType.SingleValue)]
        public string Name { get; set; }

        public int OnExecute()
        {
            return _configuration.GetValue<string>("abcd") == "1234" ? 0 : -1;
        }
    }

    class Convention : ICommandLineConvention, IServiceConvention, IConfigurationConvention
    {
        public void Register(ICommandLineConventionContext context)
        {
            context.AddCommand<MyCommand>("my");
        }

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
