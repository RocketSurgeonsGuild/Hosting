using IRsgHostingEnvironment = Rocket.Surgery.Hosting.IHostingEnvironment;
using RsgHostingEnvironment = Rocket.Surgery.Hosting.HostingEnvironment;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Hosting
{
    public static class HostingEnvironmentExtensions
    {
        public static IRsgHostingEnvironment ToRocketSurgeryHostingEnvironment(this IHostingEnvironment environment)
        {
            return new RsgHostingEnvironment(
                environment.EnvironmentName,
                environment.ApplicationName,
                environment.WebRootPath,
                environment.ContentRootPath);
        }
    }
}
