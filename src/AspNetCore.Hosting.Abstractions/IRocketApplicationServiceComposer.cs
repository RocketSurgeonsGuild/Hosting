using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.Hosting
{
    public interface IRocketApplicationServiceComposer
    {
        void ComposeServices(IServiceCollection services, IDictionary<object, object> properties, out IServiceProvider systemServiceProvider, out IServiceProvider applicationServiceProvider);
    }
}
