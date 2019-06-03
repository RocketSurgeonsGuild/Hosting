using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.DependencyInjection;
using ConfigurationBuilder = Rocket.Surgery.Extensions.Configuration.ConfigurationBuilder;

namespace Rocket.Surgery.Hosting.Functions
{
    public static class RocketSurgeryWebJobsBuilderExtensions
    {
        internal static void BuildContainer(ILogger logger, IServiceCollection services, Assembly assembly, object startupInstance, Action<IConventionHostBuilder> buildAction)
        {
            var environmentNames = new[]
            {
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                Environment.GetEnvironmentVariable("WEBSITE_SLOT_NAME"),
                "Unknown"
            };

            var applicationNames = new[]
            {
                Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"),
                "Functions"
            };

            var envionment = new RocketEnvironment(
                environmentNames.First(x => !string.IsNullOrEmpty(x)),
                applicationNames.First(x => !string.IsNullOrEmpty(x)),
                contentRootPath: null,
                contentRootFileProvider: null
            );

            var location = Path.GetDirectoryName(assembly.Location);
            DependencyContext context = null;
            while (context == null && !string.IsNullOrEmpty(location))
            {
                var depsFilePath = Path.Combine(location, assembly.GetName().Name + ".deps.json");
                if (File.Exists(depsFilePath))
                {
                    using (var stream = File.Open(depsFilePath, FileMode.Open, FileAccess.Read))
                    {
                        context = new DependencyContextJsonReader().Read(stream);
                        break;
                    }
                }
                location = Path.GetDirectoryName(location);
            }

            // var context = DependencyContext.Load(assembly);
            var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(context, logger);
            var assemblyProvider = new DependencyContextAssemblyProvider(context, logger);
            var scanner = new AggregateConventionScanner(assemblyCandidateFinder);
            var properties = new Dictionary<object, object>();
            var diagnosticSource = new DiagnosticListener("Rocket.Surgery.Azure");

            var hostingContext = new RocketHostingContext(assembly, scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, properties);

            buildAction(hostingContext);

            if (startupInstance is IConvention convention)
            {
                scanner.AppendConvention(convention);
            }

            var extBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
            var configurationBuilder = new ConfigurationBuilder(
                scanner,
                envionment,
                new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(),
                extBuilder,
                diagnosticSource,
                properties
            );

            configurationBuilder.Build();
            var configuration = extBuilder
                .AddEnvironmentVariables()
                .Build();

            services.AddLogging();

            var diBuilder = new ServicesBuilder(
                scanner,
                assemblyProvider,
                assemblyCandidateFinder,
                services,
                configuration,
                envionment,
                diagnosticSource,
                properties
            );
            new ConventionComposer(scanner)
                .Register(diBuilder, typeof(IServiceConvention), typeof(ServiceConventionDelegate));
        }

        public static IWebJobsBuilder AddRocketSurgery(this IWebJobsBuilder builder, Assembly assembly, object startupInstance, Action<IConventionHostBuilder> buildAction)
        {
            var logger = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("WebJobsBuilder");

            BuildContainer(logger, builder.Services, assembly, startupInstance, buildAction);

            return builder;
        }
    }
}
