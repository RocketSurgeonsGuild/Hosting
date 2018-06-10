using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Rocket.Surgery.Hosting
{
    class ServicesBuilderServiceProviderFactory : IServiceProviderFactory<IServicesBuilder>
    {
        private readonly Func<IServiceCollection, IServicesBuilder> _func;

        public ServicesBuilderServiceProviderFactory(
            Func<IServiceCollection, IServicesBuilder> func)
        {
            _func = func;
        }

        public IServicesBuilder CreateBuilder(IServiceCollection services)
        {
            return _func(services);
        }

        public IServiceProvider CreateServiceProvider(IServicesBuilder containerBuilder)
        {
            foreach (var d in containerBuilder.Services
                .Where(x => x.ServiceType == typeof(IRocketHostBuilder) || x.ServiceType == typeof(IHostBuilder))
                .ToArray())
            {
                containerBuilder.Services.Remove(d);
            }

            return containerBuilder.Build();
        }
    }
}
