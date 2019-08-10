using System.Collections.Generic;
using System.Diagnostics;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

namespace Rocket.Surgery.Hosting.Functions
{
    /// <summary>
    /// Interface IRocketWebJobsContext
    /// Implements the <see cref="IConventionHostBuilder" />
    /// </summary>
    /// <seealso cref="IConventionHostBuilder" />
    public interface IRocketWebJobsContext : IConventionHostBuilder
    {
        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>The properties.</value>
        IDictionary<object, object> Properties { get; }
    }
}
