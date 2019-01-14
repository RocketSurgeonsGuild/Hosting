using Autofac;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Extensions.Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Surgery.Hosting
{
    public static class AutofacRocketHostExtensions
    {
        public static IHostBuilder UseAutofac(this IHostBuilder builder, ContainerBuilder containerBuilder = null)
        {
            ((IRocketHostBuilder)builder).UseAutofac();
            return builder;
        }

        public static IRocketHostBuilder UseAutofac(this IRocketHostBuilder builder, ContainerBuilder containerBuilder = null)
        {
            builder
                .UseServicesBuilderFactory((conventionScanner, provider, finder, services, configuration, environment, logger1, properties) =>
                new AutofacBuilder(
                    containerBuilder ?? new ContainerBuilder(),
                    builder.Scanner, 
                    builder.AssemblyProvider,
                    builder.AssemblyCandidateFinder, 
                    services, 
                    configuration, 
                    environment, 
                    builder.DiagnosticSource,
                    builder.Properties));

            return builder;
        }

        public static T ContributeAutofac<T>(this T builder, AutofacConventionDelegate loggingConventionDelegate)
            where T : IRocketHostBuilder
        {
            builder.AppendDelegate(loggingConventionDelegate);
            return builder;
        }
    }
}
