﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Hosting;

namespace Extensions.Host
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(DependencyContext.Default);
            var assemblyProvider = new DependencyContextAssemblyProvider(DependencyContext.Default);
            var diagnosticSource = new DiagnosticListener("Extensions.Host");
            var builder = new RocketHostBuilder(new HostBuilder(), new AggregateConventionScanner(assemblyCandidateFinder),
                assemblyCandidateFinder, assemblyProvider, diagnosticSource, args ?? Array.Empty<string>());

            builder.AppendConvention(new Convention());

            return builder.ContributeCommandLine(c => c.OnRun(x =>
            {
                Console.WriteLine($"               Debug: {x.Debug}");
                Console.WriteLine($"               Trace: {x.Trace}");
                Console.WriteLine($"            LogLevel: {x.GetLogLevel()}");
                Console.WriteLine($"             Verbose: {x.Verbose}");
                Console.WriteLine($"    IsDefaultCommand: {x.IsDefaultCommand}");
                Console.WriteLine($"  RemainingArguments: {string.Join(" ", x.RemainingArguments)}");
                return 1234;
            }))
                .GoAsync();
            //.RunCliOrStart();
        }
    }

    [Command(ThrowOnUnexpectedArgument =  false)]
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