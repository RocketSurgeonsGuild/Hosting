using System.Collections.Generic;
using System.Diagnostics;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

namespace Rocket.Surgery.Hosting
{
    public interface IRocketHostingContext
    {
        IConventionScanner Scanner { get; }
        DiagnosticSource DiagnosticSource { get; }
        IServiceProviderDictionary ServiceProperties { get; }
        IAssemblyProvider AssemblyProvider { get; }
        IAssemblyCandidateFinder AssemblyCandidateFinder { get; }
        string[] Arguments { get; }
    }
}
