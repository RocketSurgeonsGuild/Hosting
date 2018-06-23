using System;
using System.Threading.Tasks;
using FluentAssertions;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Rocket.Surgery.AspNetCore.Hosting.Cli;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests
{
    [Command]
    class MyCommand
    {
        private readonly IWebHost _webHost;
        private readonly WebHostWrapper _webHostWrapper;

        public MyCommand(IWebHost webHost, WebHostWrapper webHostWrapper)
        {
            _webHost = webHost ?? throw new ArgumentNullException(nameof(webHost));
            _webHostWrapper = webHostWrapper ?? throw new ArgumentNullException(nameof(webHostWrapper));
        }

        public Task<int> OnExecuteAsync()
        {
            _webHost.Should().NotBeNull();
            _webHostWrapper.CreateClient().Should().NotBeNull();
            return Task.FromResult(1234);
        }
    }
}