using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.CommandLine;

namespace Rocket.Surgery.Hosting
{
    public interface IRocketHostBuilder : IRocketSurgeryHostBuilder<IRocketHostBuilder>, IHostBuilder
    {
        new IDictionary<object, object> Properties { get; }
        /// <summary>
        /// Run the given actions to initialize the host. This can only be called once.
        /// </summary>
        /// <returns>An initialized <see cref="T:Microsoft.Extensions.Hosting.IHost" /></returns>
        new IHost Build();

        /// <summary>
        /// Sets up the configuration for the remainder of the build process and application. This can be called multiple times and
        /// the results will be additive. The results will be available at <see cref="HostBuilderContext.Configuration"/> for
        /// subsequent operations, as well as in <see cref="IHost.Services"/>.
        /// </summary>
        /// <param name="configureDelegate"></param>
        /// <returns>The same instance of the <see cref="T"/> for chaining.</returns>
        new IRocketHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate);

        /// <summary>
        /// Enables configuring the instantiated dependency container. This can be called multiple times and
        /// the results will be additive.
        /// </summary>
        /// <typeparam name="TContainerBuilder"></typeparam>
        /// <param name="configureDelegate"></param>
        /// <returns>The same instance of the <see cref="T"/> for chaining.</returns>
        new IRocketHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate);

        /// <summary>
        /// Set up the configuration for the builder itself. This will be used to initialize the <see cref="IHostingEnvironment"/>
        /// for use later in the build process. This can be called multiple times and the results will be additive.
        /// </summary>
        /// <param name="configureDelegate"></param>
        /// <returns>The same instance of the <see cref="T"/> for chaining.</returns>
        new IRocketHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate);

        /// <summary>
        /// Adds services to the container. This can be called multiple times and the results will be additive.
        /// </summary>
        /// <param name="configureDelegate"></param>
        /// <returns>The same instance of the <see cref="T"/> for chaining.</returns>
        new IRocketHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);

        IRocketHostBuilder  UseServicesBuilderFactory(ServicesBuilderDelegate configureDelegate);

        IHostBuilder AsHostBuilder();
    }
}
