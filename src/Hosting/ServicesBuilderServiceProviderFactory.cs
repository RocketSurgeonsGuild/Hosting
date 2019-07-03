using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Rocket.Surgery.Hosting
{
    /// <summary>
    /// Class ServicesBuilderServiceProviderFactory.
    /// Implements the <see cref="IServiceProviderFactory{IServicesBuilder}" />
    /// </summary>
    /// <seealso cref="IServiceProviderFactory{IServicesBuilder}" />
    class ServicesBuilderServiceProviderFactory : IServiceProviderFactory<IServicesBuilder>
    {
        private readonly Func<IServiceCollection, IServicesBuilder> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServicesBuilderServiceProviderFactory"/> class.
        /// </summary>
        /// <param name="func">The function.</param>
        public ServicesBuilderServiceProviderFactory(Func<IServiceCollection, IServicesBuilder> func)
        {
            _func = func;
        }

        /// <summary>
        /// Creates a container builder from an <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">The collection of services</param>
        /// <returns>A container builder that can be used to create an <see cref="T:System.IServiceProvider" />.</returns>
        public IServicesBuilder CreateBuilder(IServiceCollection services)
        {
            return _func(services);
        }

        /// <summary>
        /// Creates the service provider.
        /// </summary>
        /// <param name="containerBuilder">The container builder.</param>
        /// <returns>IServiceProvider.</returns>
        public IServiceProvider CreateServiceProvider(IServicesBuilder containerBuilder)
        {
            var exec = ((IConventionContext)containerBuilder).Properties[typeof(ICommandLineExecutor)] as ICommandLineExecutor;
            if (exec != null)
            {
                var result = new CommandLineResult();
                containerBuilder.Services.AddSingleton(result);
                containerBuilder.Services.AddSingleton(exec.ApplicationState);
                // Remove the hosted service that bootstraps kestrel, we are executing a command here.
                var webHostedServices = containerBuilder.Services
                    .Where(x => x.ImplementationType?.FullName.Contains("Microsoft.AspNetCore.Hosting.Internal") == true)
                    .ToArray();
                if (!exec.IsDefaultCommand)
                {
                    foreach (var descriptor in webHostedServices)
                    {
                        containerBuilder.Services.Remove(descriptor);
                    }
                }
                var hasWebHostedService = webHostedServices.Any();
                if (containerBuilder.Properties.TryGetValue(typeof(CommandLineHostedService), out var _) || !exec.IsDefaultCommand)
                {
                    containerBuilder.Services.AddSingleton<IHostedService>(_ =>
                        new CommandLineHostedService(
                            _,
                            exec,
#if NETSTANDARD2_0 || NETCOREAPP2_1
                            _.GetRequiredService<IApplicationLifetime>(),
#else
                            _.GetRequiredService<IHostApplicationLifetime>(),
#endif
                            result,
                            hasWebHostedService
                        )
                    );
                }
            }
            return containerBuilder.Build();
        }
    }
}
