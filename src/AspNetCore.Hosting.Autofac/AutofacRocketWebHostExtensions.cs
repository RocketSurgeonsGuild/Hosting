using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rocket.Surgery.AspNetCore.Hosting;
using Rocket.Surgery.AspNetCore.Hosting.Autofac;
using Rocket.Surgery.Extensions.Autofac;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Hosting
{
    public static class AutofacRocketWebHostExtensions
    {
        public static IRocketWebHostBuilder UseAutofac(this IWebHostBuilder builder, ContainerBuilder containerBuilder = null)
        {
            var conventionalBuilder = RocketWebHostExtensions.GetOrCreateBuilder(builder);
            conventionalBuilder.ApplicationServicesComposeDelegate = (b, configuration, environment) => new AutofacServiceComposer(
                b.Scanner,
                b.AssemblyProvider,
                b.AssemblyCandidateFinder,
                configuration,
                environment,
                b.DiagnosticSource,
                containerBuilder ?? new ContainerBuilder());
            conventionalBuilder.ApplicationAndSystemServicesComposeDelegate = (b, configuration, environment) => new AutofacApplicationServiceComposer(
                b.Scanner,
                b.AssemblyProvider,
                b.AssemblyCandidateFinder,
                configuration,
                environment,
                b.DiagnosticSource,
                containerBuilder ?? new ContainerBuilder());
            return conventionalBuilder;
        }
    }
}
