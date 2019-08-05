using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;

namespace Rocket.Surgery.Hosting
{
    /// <summary>
    /// Interface IRocketHostBuilder
    /// Implements the <see cref="IConventionHostBuilder{IRocketHostBuilder}" />
    /// </summary>
    /// <seealso cref="IConventionHostBuilder{IRocketHostBuilder}" />
    public interface IRocketHostBuilder : IConventionHostBuilder
    {
        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>The builder.</value>
        IHostBuilder Builder { get; }
    }
}
