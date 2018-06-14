using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.Hosting
{
    public interface IRocketServiceComposer
    {
        IServiceProvider ComposeServices(IServiceCollection services, IDictionary<object, object> properties);
    }
}
