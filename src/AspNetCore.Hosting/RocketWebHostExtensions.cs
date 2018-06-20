using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.AspNetCore.Hosting.Cli;
using Rocket.Surgery.Extensions.CommandLine;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public static class RocketWebHostExtensions
    {
        public static Task<int> RunCli(this IWebHostBuilder builder)
        {
            return ((IRocketWebHostBuilder)builder).RunCli();
        }

        public static Task<int> RunCli(this IWebHostBuilder builder, string[] args)
        {
            return ((IRocketWebHostBuilder)builder).RunCli(args);
        }
        public static Task<int> RunCliOrStart(this IWebHostBuilder builder)
        {
            return ((IRocketWebHostBuilder)builder).RunCliOrStart();
        }

        public static Task<int> RunCliOrStart(this IWebHostBuilder builder, string[] args)
        {
            return ((IRocketWebHostBuilder)builder).RunCliOrStart(args);
        }

        public static Task<int> RunCli(this IRocketWebHostBuilder builder)
        {
            return builder.RunCli(Array.Empty<string>());
        }

        public async static Task<int> RunCli(this IRocketWebHostBuilder builder, string[] args)
        {
            await Task.Yield();

            IWebHost host = null;
            WebHostWrapper webHostWrapper = null;

            builder.ConfigureServices(services =>
                services.AddSingleton(_ => webHostWrapper));
            builder.ConfigureServices(services =>
                services.AddSingleton(_ => host));

            host = builder.Build();
            webHostWrapper = new WebHostWrapper(host);

            builder.Properties[typeof(IWebHost)] = host;
            builder.Properties[typeof(WebHostWrapper)] = webHostWrapper;

            var executor = host.Services.GetRequiredService<ICommandLineExecutor>();
            return executor.Execute(host.Services);
        }

        public static Task<int> RunCliOrStart(this IRocketWebHostBuilder builder)
        {
            return builder.RunCliOrStart(Array.Empty<string>());
        }

        public static async Task<int> RunCliOrStart(this IRocketWebHostBuilder builder, string[] args)
        {
            await Task.Yield();

            IWebHost host = null;
            WebHostWrapper webHostWrapper = null;

            builder.ConfigureServices(services =>
                services.AddSingleton(_ => webHostWrapper));
            builder.ConfigureServices(services =>
                services.AddSingleton(_ => host));

            host = builder.Build();
            webHostWrapper = new WebHostWrapper(host);

            builder.Properties[typeof(IWebHost)] = host;
            builder.Properties[typeof(WebHostWrapper)] = webHostWrapper;

            var executor = host.Services.GetService<ICommandLineExecutor>();
            if (executor != null && !executor.IsDefaultCommand)
            {
                return executor.Execute(host.Services);
            }

            await host.StartAsync();
            await host.WaitForShutdownAsync();
            return 0;
        }

        public static IRocketWebHostBuilder UseCli(this IRocketWebHostBuilder hostBuilder, string[] args, Action<ICommandLineBuilder> commandLineAction = null)
        {
            hostBuilder.ConfigureAppConfiguration((context, services) =>
            {
                IApplicationState applicationState = null;
                var clb = new CommandLineBuilder(
                    hostBuilder.Scanner,
                    hostBuilder.AssemblyProvider,
                    hostBuilder.AssemblyCandidateFinder,
                    context.Configuration,
                    (IHostingEnvironment)context.HostingEnvironment,
                    hostBuilder.DiagnosticSource,
                    hostBuilder.Properties
                );
                clb.OnParse(state =>
                {
                    applicationState = state;
                    if (!state.IsDefaultCommand)
                    {
                        hostBuilder.UseServer(new CliServer());
                    }
                });
                commandLineAction?.Invoke(clb);
                var executor = clb.Build().Parse(args);

                ((IWebHostBuilder)hostBuilder).ConfigureServices(collection =>
                {
                    collection.AddSingleton(executor);
                    collection.AddSingleton(applicationState);
                });
            });
            return hostBuilder;
        }
    }
}
