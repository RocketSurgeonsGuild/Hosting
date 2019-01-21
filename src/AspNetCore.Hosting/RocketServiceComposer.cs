using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.DependencyInjection;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    class RocketServiceComposer : IRocketServiceComposer
    {
        private readonly IConventionScanner _scanner;
        private readonly IAssemblyProvider _assemblyProvider;
        private readonly IAssemblyCandidateFinder _assemblyCandidateFinder;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;
        private readonly DiagnosticSource _diagnosticSource;

        public RocketServiceComposer(
            IConventionScanner scanner,
            IAssemblyProvider assemblyProvider,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            IConfiguration configuration,
            IHostingEnvironment environment,
            DiagnosticSource diagnosticSource)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            _assemblyProvider = assemblyProvider ?? throw new ArgumentNullException(nameof(assemblyProvider));
            _assemblyCandidateFinder = assemblyCandidateFinder ?? throw new ArgumentNullException(nameof(assemblyCandidateFinder));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _diagnosticSource = diagnosticSource ?? throw new ArgumentNullException(nameof(diagnosticSource));
        }

        public void ComposeServices(
            IServiceCollection services,
            IDictionary<object, object> properties,
            out IServiceProvider systemServiceProvider,
            out IServiceProvider applicationServiceProvider)
        {
            var builder = new ServicesBuilder(
                _scanner,
                _assemblyProvider,
                _assemblyCandidateFinder,
                services,
                _configuration,
                _environment,
                _diagnosticSource,
                properties);

            var c = builder.Build();

            systemServiceProvider = null;
            applicationServiceProvider = c;
        }
    }
}
