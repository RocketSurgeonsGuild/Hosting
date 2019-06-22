using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Extensions
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            var diagnosticSource = new DiagnosticListener("Extensions.Host");

            var builder = Host.CreateDefaultBuilder(args)
                .LaunchWith(RocketBooster.For(DependencyContext.Default, diagnosticSource))
                .ConfigureRocketSurgey(b => b.AppendConvention(new Convention()))
                .ConfigureWebHostDefaults(x => x
                    .UseKestrel()
                    .UseStartup<Startup>()
                );
            return builder
                .RunCli();
        }
    }

    [Command(ThrowOnUnexpectedArgument = false)]
    class MyCommand
    {
        private readonly IConfiguration _configuration;
        private readonly IApplicationState x;

        public MyCommand(IConfiguration configuration, IApplicationState x)
        {
            _configuration = configuration;
            this.x = x;
        }
        [Option(CommandOptionType.SingleValue)]
        public string Name { get; set; }

        public int OnExecute()
        {
            Console.WriteLine($"               Debug: {x.Debug}");
            Console.WriteLine($"               Trace: {x.Trace}");
            Console.WriteLine($"            LogLevel: {x.GetLogLevel()}");
            Console.WriteLine($"             Verbose: {x.Verbose}");
            Console.WriteLine($"    IsDefaultCommand: {x.IsDefaultCommand}");
            Console.WriteLine($"  RemainingArguments: {string.Join(" ", x.RemainingArguments)}");
            return _configuration.GetValue<string>("abcd") == "1234" ? 0 : -1;
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Use((context, func) => context.Response.WriteAsync("TestStartup -> Compose"));
        }
    }

    class Convention : IServiceConvention, IConfigurationConvention, ICommandLineConvention
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
