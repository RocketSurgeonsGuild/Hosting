using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Hosting;

namespace Rocket.Surgery.CommandLine
{
    class CommandLineHost
    {
        private readonly IHostBuilder _builder;
        private string[] _args;
        public CommandLineHost(IHostBuilder builder)
        {
            _builder = builder;
        }

        public void CaptureAndRemoveArguments(IConfigurationBuilder configurationBuilder)
        {
            var commandLineSource = configurationBuilder.Sources.OfType<CommandLineConfigurationSource>()
                .FirstOrDefault();
            if (commandLineSource != null)
            {
                configurationBuilder.Sources.Remove(commandLineSource);
                _args = commandLineSource.Args.ToArray();
            }
        }

        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var conventionalBuilder = RocketHostExtensions.GetOrCreateBuilder(_builder);
            services.Configure<ConsoleLifetimeOptions>(c => c.SuppressStatusMessages = true);
            services.AddSingleton<IRocketHostingContext>(_ => new RocketHostingContext(conventionalBuilder, _args ?? Array.Empty<string>()));
        }
    }
}
