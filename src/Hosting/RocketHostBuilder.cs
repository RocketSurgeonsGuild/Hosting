using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

namespace Rocket.Surgery.Hosting
{
    /// <summary>
    /// Class RocketHostBuilder.
    /// Implements the <see cref="ConventionHostBuilder{IRocketHostBuilder}" />
    /// Implements the <see cref="IRocketHostBuilder" />
    /// </summary>
    /// <seealso cref="ConventionHostBuilder{IRocketHostBuilder}" />
    /// <seealso cref="IRocketHostBuilder" />
    class RocketHostBuilder : ConventionHostBuilder<IRocketHostBuilder>, IRocketHostBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RocketHostBuilder"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="scanner">The scanner.</param>
        /// <param name="assemblyCandidateFinder">The assembly candidate finder.</param>
        /// <param name="assemblyProvider">The assembly provider.</param>
        /// <param name="diagnosticSource">The diagnostic source.</param>
        /// <param name="serviceProperties">The service properties.</param>
        public RocketHostBuilder(IHostBuilder builder, IConventionScanner scanner, IAssemblyCandidateFinder assemblyCandidateFinder, IAssemblyProvider assemblyProvider, DiagnosticSource diagnosticSource, IServiceProviderDictionary serviceProperties) : base(scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, serviceProperties)
        {
            Builder = builder;
            Logger = new DiagnosticLogger(diagnosticSource);
        }

        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>The builder.</value>
        public IHostBuilder Builder { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public ILogger Logger { get; }

        /// <summary>
        /// Withes the specified scanner.
        /// </summary>
        /// <param name="scanner">The scanner.</param>
        /// <returns>RocketHostBuilder.</returns>
        internal RocketHostBuilder With(IConventionScanner scanner)
        {
            return new RocketHostBuilder(Builder, scanner, AssemblyCandidateFinder, AssemblyProvider, DiagnosticSource, ServiceProperties);
        }

        /// <summary>
        /// Withes the specified assembly candidate finder.
        /// </summary>
        /// <param name="assemblyCandidateFinder">The assembly candidate finder.</param>
        /// <returns>RocketHostBuilder.</returns>
        internal RocketHostBuilder With(IAssemblyCandidateFinder assemblyCandidateFinder)
        {
            return new RocketHostBuilder(Builder, Scanner, assemblyCandidateFinder, AssemblyProvider, DiagnosticSource, ServiceProperties);
        }

        /// <summary>
        /// Withes the specified assembly provider.
        /// </summary>
        /// <param name="assemblyProvider">The assembly provider.</param>
        /// <returns>RocketHostBuilder.</returns>
        internal RocketHostBuilder With(IAssemblyProvider assemblyProvider)
        {
            return new RocketHostBuilder(Builder, Scanner, AssemblyCandidateFinder, assemblyProvider, DiagnosticSource, ServiceProperties);
        }

        /// <summary>
        /// Withes the specified diagnostic source.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source.</param>
        /// <returns>RocketHostBuilder.</returns>
        internal RocketHostBuilder With(DiagnosticSource diagnosticSource)
        {
            return new RocketHostBuilder(Builder, Scanner, AssemblyCandidateFinder, AssemblyProvider, diagnosticSource, ServiceProperties);
        }
    }
}
