using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting
{
    // To be replaced by Host in 3.x
    public static class RocketHost
    {
        public static IHostBuilder CreateDefaultBuilder(string[] args = null)
        {
            var builder = new HostBuilder();

            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder.ConfigureHostConfiguration(config =>
            {
                config.AddEnvironmentVariables(); // TODO: Prefix?
                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            });

            builder.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    // Configure the base path (fix coming in 3.0.0)
                    config.SetBasePath(env.ContentRootPath);

                    config
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                    {
                        var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                        if (appAssembly != null)
                        {
                            config.AddUserSecrets(appAssembly, optional: true);
                        }
                    }

                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                });

            return builder;
        }
    }
}
