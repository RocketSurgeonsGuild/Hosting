using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Hosting;
using IConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public partial class RocketWebHostBuilder : IRocketHostBuilder
    {
        IRocketHostBuilder IRocketWebHostBuilder.AsRocketHostBuilder() => this;
        IHostBuilder IRocketWebHostBuilder.AsHostBuilder() => this;

        IRocketHostBuilder IRocketSurgeryHostBuilder<IRocketHostBuilder>.PrependDelegate(Delegate @delegate)
        {
            Scanner.PrependDelegate(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        IRocketHostBuilder IRocketSurgeryHostBuilder<IRocketHostBuilder>.PrependConvention(IConvention convention)
        {
            Scanner.PrependConvention(convention ?? throw new ArgumentNullException(nameof(convention)));
            return this;
        }

        IRocketHostBuilder IRocketSurgeryHostBuilder<IRocketHostBuilder>.AppendDelegate(Delegate @delegate)
        {
            Scanner.AppendDelegate(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        IRocketHostBuilder IRocketSurgeryHostBuilder<IRocketHostBuilder>.AppendConvention(IConvention convention)
        {
            Scanner.AppendConvention(convention ?? throw new ArgumentNullException(nameof(convention)));
            return this;
        }

        IRocketHostBuilder IRocketSurgeryHostBuilder<IRocketHostBuilder>.ExceptConvention(Type type)
        {
            Scanner.ExceptConvention(type ?? throw new ArgumentNullException(nameof(type)));
            return this;
        }

        IRocketHostBuilder IRocketSurgeryHostBuilder<IRocketHostBuilder>.ExceptConvention(Assembly assembly)
        {
            Scanner.ExceptConvention(assembly ?? throw new ArgumentNullException(nameof(assembly)));
            return this;
        }

        IRocketHostBuilder IRocketHostBuilder.ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            throw new NotSupportedException("Not supported on web hosts");
        }

        IRocketHostBuilder IRocketHostBuilder.ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            throw new NotSupportedException("Not supported on web hosts");
        }

        IRocketHostBuilder IRocketHostBuilder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if (configureDelegate == null)
            {
                throw new ArgumentNullException(nameof(configureDelegate));
            }

            Scanner.AppendDelegate(new ServiceConventionDelegate(context =>
            {
                configureDelegate(new HostBuilderContext(context.Properties)
                {
                    HostingEnvironment = context.Environment,
                    Configuration = context.Configuration
                }, context.Services);
            }));
            return this;
        }

        IRocketHostBuilder IRocketHostBuilder.UseServicesBuilderFactory(ServicesBuilderDelegate configureDelegate)
        {
            ((IRocketWebHostBuilder) this).UseServicesBuilderFactory(configureDelegate);
            return this;
        }

        IRocketHostBuilder IRocketHostBuilder.ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            if (configureDelegate == null)
            {
                throw new ArgumentNullException(nameof(configureDelegate));
            }

            Scanner.AppendDelegate(new ConfigurationConventionDelegate(context =>
            {
                configureDelegate(new HostBuilderContext(context.Properties)
                {
                    HostingEnvironment = context.Environment,
                    Configuration = context.Configuration
                }, context);
            }));
            return this;
        }

        IHostBuilder IRocketHostBuilder.AsHostBuilder()
        {
            return this;
        }

        IHost IRocketHostBuilder.Build()
        {
            return BuildHost();
        }

        IHostBuilder IHostBuilder.ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            ((IRocketHostBuilder)this).ConfigureHostConfiguration(configureDelegate);
            return this;
        }

        IHostBuilder IHostBuilder.ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            ((IRocketHostBuilder)this).ConfigureAppConfiguration(configureDelegate);
            return this;
        }

        IHostBuilder IHostBuilder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            ((IRocketHostBuilder)this).ConfigureServices(configureDelegate);
            return this;
        }

        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            throw new NotSupportedException("Not supported on web hosts");
        }

        IHostBuilder IHostBuilder.ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            throw new NotSupportedException("Not supported on web hosts");
        }

        IHost IHostBuilder.Build()
        {
            return BuildHost();
        }
    }
}
