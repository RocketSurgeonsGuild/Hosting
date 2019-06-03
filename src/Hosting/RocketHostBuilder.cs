using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

namespace Rocket.Surgery.Hosting
{
    class RocketHostBuilder : ConventionHostBuilder<IRocketHostBuilder>, IRocketHostBuilder
    {
        public RocketHostBuilder(IHostBuilder builder, IConventionScanner scanner, IAssemblyCandidateFinder assemblyCandidateFinder, IAssemblyProvider assemblyProvider, DiagnosticSource diagnosticSource, IDictionary<object, object> properties) : base(scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, properties)
        {
            Builder = builder;
        }

        public IHostBuilder Builder { get; }

        internal RocketHostBuilder With(IConventionScanner scanner)
        {
            return new RocketHostBuilder(Builder, scanner, AssemblyCandidateFinder, AssemblyProvider, DiagnosticSource, Properties);
        }

        internal RocketHostBuilder With(IAssemblyCandidateFinder assemblyCandidateFinder)
        {
            return new RocketHostBuilder(Builder, Scanner, assemblyCandidateFinder, AssemblyProvider, DiagnosticSource, Properties);
        }

        internal RocketHostBuilder With(IAssemblyProvider assemblyProvider)
        {
            return new RocketHostBuilder(Builder, Scanner, AssemblyCandidateFinder, assemblyProvider, DiagnosticSource, Properties);
        }

        internal RocketHostBuilder With(DiagnosticSource diagnosticSource)
        {
            return new RocketHostBuilder(Builder, Scanner, AssemblyCandidateFinder, AssemblyProvider, diagnosticSource, Properties);
        }
    }
}
