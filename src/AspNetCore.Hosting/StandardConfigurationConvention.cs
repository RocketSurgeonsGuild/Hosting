using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Extensions.Configuration;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public class StandardConfigurationConvention : IConfigurationConvention
    {
        public void Register(IConfigurationConventionContext context)
        {
            var env = context.Environment;

            context
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddYamlFile("appsettings.yml", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.yml", optional: true, reloadOnChange: true)
                ;

            if (env.IsDevelopment())
            {
                var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                if (appAssembly != null)
                {
                    context.AddUserSecrets(appAssembly, optional: true);
                }
            }
        }
    }
}
