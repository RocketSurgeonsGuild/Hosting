using System.Reflection;
using Microsoft.Azure.WebJobs;
using Rocket.Surgery.Conventions;

namespace Rocket.Surgery.Hosting.Functions
{
    /// <summary>
    /// Interface IRocketFunctionHostBuilder
    /// Implements the <see cref="IConventionHostBuilder{IRocketFunctionHostBuilder}" />
    /// </summary>
    /// <seealso cref="IConventionHostBuilder{IRocketFunctionHostBuilder}" />
    public interface IRocketFunctionHostBuilder : IConventionHostBuilder<IRocketFunctionHostBuilder>
    {
        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>The builder.</value>
        IWebJobsBuilder Builder { get; }

        /// <summary>
        /// Gets the functions assembly.
        /// </summary>
        /// <value>The functions assembly.</value>
        Assembly FunctionsAssembly { get; }
    }
}
