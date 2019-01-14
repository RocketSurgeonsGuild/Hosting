using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.AspNetCore.Hosting.Autofac;
using Rocket.Surgery.Extensions.Autofac;
using Rocket.Surgery.Hosting;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public static class AutofacRocketWebHostExtensions
    {
        public static IWebHostBuilder UseAutofac(this IWebHostBuilder builder, ContainerBuilder containerBuilder = null)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IRocketApplicationServiceComposer>(_ =>
                ActivatorUtilities.CreateInstance<AutofacApplicationServiceComposer>(_, containerBuilder ?? new ContainerBuilder()));
                services.AddSingleton<IRocketServiceComposer>(_ =>
                ActivatorUtilities.CreateInstance<AutofacServiceComposer>(_, containerBuilder ?? new ContainerBuilder()));
            });
            return builder;
        }

        public static T ContributeAutofac<T>(this T builder, AutofacConventionDelegate loggingConventionDelegate)
            where T : IRocketWebHostBuilder
        {
            builder.AppendDelegate(loggingConventionDelegate);
            return builder;
        }
    }
}
