using Autofac;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Extensions.Autofac;
using System;
using System.Collections.Generic;
using System.Text;
using Rocket.Surgery.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting
{
    public static class AutofacRocketHostExtensions
    {
        public static IRocketHostBuilder UseAutofac(this IHostBuilder builder, ContainerBuilder containerBuilder = null)
        {
            builder.ConfigureServices((context, services) =>
            {
                var conventionalBuilder = RocketHostExtensions.GetOrCreateBuilder(builder);
                builder.UseServiceProviderFactory(
                    new ServicesBuilderServiceProviderFactory(collection =>
                        new AutofacBuilder(
                            containerBuilder ?? new ContainerBuilder(),
                            conventionalBuilder.Scanner,
                            conventionalBuilder.AssemblyProvider,
                            conventionalBuilder.AssemblyCandidateFinder,
                            collection,
                            context.Configuration,
                            context.HostingEnvironment,
                            conventionalBuilder.DiagnosticSource,
                            conventionalBuilder.Properties
                        )
                    )
                );
            });
            return RocketHostExtensions.GetOrCreateBuilder(builder);
        }
    }
}
