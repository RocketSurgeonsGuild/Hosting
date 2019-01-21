using System.Collections.Generic;
using System.Diagnostics;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public interface IRocketHostingContext
    {
        IConventionScanner Scanner { get; }
        DiagnosticSource DiagnosticSource { get; }
        IDictionary<object, object> Properties { get; }
        IAssemblyProvider AssemblyProvider { get; }
        IAssemblyCandidateFinder AssemblyCandidateFinder { get; }
    }
}
