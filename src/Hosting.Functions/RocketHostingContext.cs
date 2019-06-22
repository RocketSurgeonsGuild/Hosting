using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
//using Microsoft.Azure.WebJobs.Hosting;

//[assembly: WebJobsStartup(typeof(RocketSurgeryWebJobsStartup))]

namespace Rocket.Surgery.Hosting.Functions
{
    class RocketHostingContext : ConventionHostBuilder<IRocketWebJobsContext>, IRocketWebJobsContext, IConventionHostBuilder
    {
        public RocketHostingContext(
            Assembly assembly,
            IConventionScanner scanner,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            IAssemblyProvider assemblyProvider,
            DiagnosticSource diagnosticSource,
            IDictionary<object, object> properties): base(scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, properties)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            Scanner = scanner;
            AssemblyProvider = assemblyProvider;
            AssemblyCandidateFinder = assemblyCandidateFinder;
            DiagnosticSource = diagnosticSource;
            Properties = properties;
        }

        public Assembly Assembly { get; }

        public IConventionScanner Scanner { get; }

        public DiagnosticSource DiagnosticSource { get; }

        public IDictionary<object, object> Properties { get; }

        public IAssemblyProvider AssemblyProvider { get; }

        public IAssemblyCandidateFinder AssemblyCandidateFinder { get; }
    }
}
