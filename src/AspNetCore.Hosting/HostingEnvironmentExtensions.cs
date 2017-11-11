// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Extensions.Hosting
{
    public static class HostingEnvironmentExtensions
    {
        public static IHostingEnvironment ToRocketSurgeryHostingEnvironment(this Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            return new HostingEnvironment(
                environment.EnvironmentName,
                environment.ApplicationName,
                environment.WebRootPath,
                environment.ContentRootPath);
        }
    }
}
