using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.CommandLine;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Rocket.Surgery.Hosting
{
    class ServicesBuilderServiceProviderFactory : IServiceProviderFactory<IServicesBuilder>
    {
        private readonly Func<IServiceCollection, IServicesBuilder> _func;

        public ServicesBuilderServiceProviderFactory(Func<IServiceCollection, IServicesBuilder> func)
        {
            _func = func;
        }

        public IServicesBuilder CreateBuilder(IServiceCollection services)
        {
            return _func(services);
        }

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
                containerBuilder.Services.AddSingleton<IHostedService>(_ =>
                    new CommandLineHostedService(_, exec, _.GetRequiredService<IHostApplicationLifetime>(), result, webHostedServices.Any()));
                if (!exec.IsDefaultCommand)
                {
                    foreach (var descriptor in webHostedServices)
                    {
                        containerBuilder.Services.Remove(descriptor);
                    }
                }
            }
            return containerBuilder.Build();
        }
    }
}
