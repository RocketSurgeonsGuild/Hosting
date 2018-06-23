using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Rocket.Surgery.Hosting.Tests
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
}
