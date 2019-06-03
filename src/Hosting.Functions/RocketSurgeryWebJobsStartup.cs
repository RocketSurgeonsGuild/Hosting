using System.Reflection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Rocket.Surgery.Conventions;
//using Microsoft.Azure.WebJobs.Hosting;

//[assembly: WebJobsStartup(typeof(RocketSurgeryWebJobsStartup))]

namespace Rocket.Surgery.Hosting.Functions
{
    public abstract class RocketSurgeryWebJobsStartup : IWebJobsStartup
    {
        private readonly Assembly _assembly;

        public RocketSurgeryWebJobsStartup(Assembly assembly)
        {
            _assembly = assembly;
        }

        public virtual void Configure(IWebJobsBuilder builder)
        {
            builder.AddRocketSurgery(_assembly, this, OnBuild);
        }

        protected abstract void OnBuild(IConventionHostBuilder builder);
    }
}
