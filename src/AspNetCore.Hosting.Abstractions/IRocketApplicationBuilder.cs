using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Rocket.Surgery.Builders;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    /// <summary>
    /// Interface ICoreApplicationBuilder
    /// </summary>
    /// TODO Edit XML Comment Template for ICoreApplicationBuilder
    public interface IRocketApplicationBuilder : IApplicationBuilder, IBuilder
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        /// TODO Edit XML Comment Template for Configuration
        IConfiguration Configuration { get; }
    }
}
