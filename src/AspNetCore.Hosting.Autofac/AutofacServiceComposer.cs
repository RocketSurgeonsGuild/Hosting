using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Autofac;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Rocket.Surgery.AspNetCore.Hosting.Autofac
{
    public class AutofacServiceComposer : IRocketServiceComposer
    {
        private readonly IConventionScanner _scanner;
        private readonly IAssemblyProvider _assemblyProvider;
        private readonly IAssemblyCandidateFinder _assemblyCandidateFinder;
        private readonly IConfiguration _configuration;
        private readonly Microsoft.Extensions.Hosting.IHostingEnvironment _environment;
        private readonly DiagnosticSource _diagnosticSource;
        private readonly ContainerBuilder _containerBuilder;

        public AutofacServiceComposer(
            IConventionScanner scanner,
            IAssemblyProvider assemblyProvider,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            IConfiguration configuration,
            Microsoft.Extensions.Hosting.IHostingEnvironment environment,
            DiagnosticSource diagnosticSource,
            ContainerBuilder containerBuilder)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            _assemblyProvider = assemblyProvider ?? throw new ArgumentNullException(nameof(assemblyProvider));
            _assemblyCandidateFinder = assemblyCandidateFinder ?? throw new ArgumentNullException(nameof(assemblyCandidateFinder));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _diagnosticSource = diagnosticSource ?? throw new ArgumentNullException(nameof(diagnosticSource));
            _containerBuilder = containerBuilder ?? throw new ArgumentNullException(nameof(containerBuilder));
        }

        public void ComposeServices(IServiceCollection services, IDictionary<object, object> properties, out IServiceProvider systemServiceProvider,
            out IServiceProvider applicationServiceProvider)
        {
            var builder = new AutofacBuilder(
                _containerBuilder,
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
            applicationServiceProvider = c.Resolve<IServiceProvider>();
        }
    }
}
