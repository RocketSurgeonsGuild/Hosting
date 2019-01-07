using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Rocket.Surgery.Extensions.Configuration;

namespace Rocket.Surgery.Hosting
{
    public class CliConfigurationConvention : IConfigurationConvention
    {
        public void Register(IConfigurationConventionContext context)
        {
            var env = context.Environment;

            var fp = new PhysicalFileProvider(Directory.GetCurrentDirectory());

            context
                .AddJsonFile(provider: fp, "appsettings.json", optional: true, reloadOnChange: true)
                .AddYamlFile(provider: fp, "appsettings.yml", optional: true, reloadOnChange: true)
                .AddJsonFile(provider: fp, $"appsettings.{env.EnvironmentName.ToLower()}.json", optional: true, reloadOnChange: true)
                .AddYamlFile(provider: fp, $"appsettings.{env.EnvironmentName.ToLower()}.yml", optional: true, reloadOnChange: true)
                ;
        }
    }
}
