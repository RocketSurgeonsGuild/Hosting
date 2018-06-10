using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public class RocketServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        private readonly Func<IServiceCollection, IServicesBuilder> _func;

        public RocketServiceProviderFactory(
            Func<IServiceCollection, IServicesBuilder> func)
        {
            _func = func;
        }

        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            return services;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection services)
        {
            return _func(services).Build();
        }
    }
}
