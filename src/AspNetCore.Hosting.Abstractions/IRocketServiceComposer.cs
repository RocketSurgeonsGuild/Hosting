using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public interface IRocketServiceComposer
    {
        void ComposeServices(IServiceCollection services, IDictionary<object, object> properties, out IServiceProvider systemServiceProvider, out IServiceProvider applicationServiceProvider);
    }
}
