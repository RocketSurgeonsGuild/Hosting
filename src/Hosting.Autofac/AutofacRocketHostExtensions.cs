﻿using Autofac;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Extensions.Autofac;
using System;
using System.Collections.Generic;
using System.Text;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Class AutofacRocketHostExtensions.
    /// </summary>
    public static class AutofacRocketHostExtensions
    {
        /// <summary>
        /// Uses the autofac.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="containerBuilder">The container builder.</param>
        /// <returns>IRocketHostBuilder.</returns>
        public static IRocketHostBuilder UseAutofac(this IRocketHostBuilder builder, ContainerBuilder containerBuilder = null)
        {
            builder.Builder.ConfigureServices((context, services) =>
            {
                var conventionalBuilder = RocketHostExtensions.GetOrCreateBuilder(builder);
                builder.Builder.UseServiceProviderFactory(
                    new ServicesBuilderServiceProviderFactory(collection =>
                        new AutofacBuilder(
                            context.HostingEnvironment.Convert(),
                            context.Configuration,
                            conventionalBuilder.Scanner,
                            conventionalBuilder.AssemblyProvider,
                            conventionalBuilder.AssemblyCandidateFinder,
                            collection,
                            containerBuilder ?? new ContainerBuilder(),
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
