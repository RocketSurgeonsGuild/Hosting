using System;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.CommandLine;

namespace Rocket.Surgery.Hosting
{
    public static class RocketHostExtensions
    {
        public static int RunCommandLine(this IRocketHostBuilder hostBuilder)
        {
            var host = hostBuilder.Build();
            return host.Services.GetRequiredService<ICommandLineExecutor>().Execute(host.Services);
        }

        public static IRocketHostBuilder UseCommandLine(this IRocketHostBuilder hostBuilder, string[] args, Action<ICommandLineBuilder> commandLineAction = null)
        {
            ICommandLineExecutor executor = null;
            IApplicationState applicationState = null;

            hostBuilder.ConfigureAppConfiguration((context, services) =>
            {
                var clb = new CommandLineBuilder(
                    hostBuilder.Scanner,
                    hostBuilder.AssemblyProvider,
                    hostBuilder.AssemblyCandidateFinder,
                    context.Configuration,
                    context.HostingEnvironment,
                    hostBuilder.DiagnosticSource,
                    hostBuilder.Properties
                );
                clb.OnParse(state => { applicationState = state; });
                commandLineAction?.Invoke(clb);
                executor = clb.Build().Parse(args);
            });

            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton(executor);
                services.AddSingleton(applicationState);
            });

            return hostBuilder;
        }
    }
}
