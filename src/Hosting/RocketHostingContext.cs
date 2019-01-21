using System.Collections.Generic;
using System.Diagnostics;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

namespace Rocket.Surgery.Hosting
{
    class RocketHostingContext : IRocketHostingContext
    {

        public RocketHostingContext(RocketHostBuilder builder, string[] args)
        {
            Arguments = args;
            Scanner = builder.Scanner;
            DiagnosticSource = builder.DiagnosticSource;
            Properties = builder.Properties;
            AssemblyProvider = builder.AssemblyProvider;
            AssemblyCandidateFinder = builder.AssemblyCandidateFinder;
        }

        public IConventionScanner Scanner { get; }

        public DiagnosticSource DiagnosticSource { get; }

        public IDictionary<object, object> Properties { get; }

        public IAssemblyProvider AssemblyProvider { get; }
        public IAssemblyCandidateFinder AssemblyCandidateFinder { get; }
        public string[] Arguments { get; }
    }
}
