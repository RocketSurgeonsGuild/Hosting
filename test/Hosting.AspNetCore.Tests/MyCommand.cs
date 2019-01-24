using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests
{
    [Command]
    class MyCommand
    {
        private readonly IServiceProvider _serviceProvider;

        public MyCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Task<int> OnExecuteAsync()
        {
            return Task.FromResult(1234);
        }
    }

    class MyStartup : IServiceConvention
    {
        public void Register(IServiceConventionContext context)
        {
            context.Services.AddSingleton(new object());
        }

        public void Configure(IApplicationBuilder builder)
        {

        }
    }
}
