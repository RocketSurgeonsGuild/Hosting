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
        public static IRocketHostBuilder UseAutofac(this IRocketHostBuilder builder, ContainerBuilder containerBuilder = null)
        {
            builder.Builder.ConfigureServices((context, services) =>
            {
                var conventionalBuilder = RocketHostExtensions.GetOrCreateBuilder(builder);
                builder.Builder.UseServiceProviderFactory(
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
