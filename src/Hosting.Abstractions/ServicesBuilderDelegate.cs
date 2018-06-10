using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Rocket.Surgery.Hosting
{
    public delegate IServicesBuilder ServicesBuilderDelegate(
        IConventionScanner scanner,
        IAssemblyProvider assemblyProvider,
        IAssemblyCandidateFinder assemblyCandidateFinder,
        IServiceCollection services,
        IConfiguration configuration,
        IHostingEnvironment environment,
        DiagnosticSource diagnosticSource,
        IDictionary<object, object> properties);
}
