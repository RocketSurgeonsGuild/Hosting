using System.Reflection;
using Microsoft.Azure.WebJobs;
using Rocket.Surgery.Conventions;

namespace Rocket.Surgery.Hosting.Functions
{
    public interface IRocketFunctionHostBuilder : IConventionHostBuilder<IRocketFunctionHostBuilder>
    {
        IWebJobsBuilder Builder { get; }
        Assembly FunctionsAssembly { get; }
    }
}
