using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using IWebHostBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;
using IWebHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using WebHostBuilderContext = Microsoft.AspNetCore.Hosting.WebHostBuilderContext;
using IMsftConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;
using MsftConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public partial class RocketWebHostBuilder
    {
        IWebHost IRocketWebHostBuilder.Build()
        {
            return Build();
        }

        IWebHost IWebHostBuilder.Build()
        {
            return Build();
        }
        IWebHostBuilder IWebHostBuilder.ConfigureServices(Action<IServiceCollection> configureServices)
        {
            _webHostBuilder.ConfigureServices(configureServices);
            return this;
        }

        IWebHostBuilder IWebHostBuilder.ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
        {
            _webHostBuilder.ConfigureServices(configureServices);
            return this;
        }

        IWebHostBuilder IWebHostBuilder.ConfigureAppConfiguration(Action<WebHostBuilderContext, IMsftConfigurationBuilder> configureDelegate)
        {
            ((IRocketWebHostBuilder)this).ConfigureAppConfiguration(configureDelegate);
            return this;
        }

        IRocketWebHostBuilder IRocketWebHostBuilder.ConfigureAppConfiguration(Action<WebHostBuilderContext, IMsftConfigurationBuilder> configureDelegate)
        {
            if (configureDelegate == null)
            {
                throw new ArgumentNullException(nameof(configureDelegate));
            }

            Scanner.AppendDelegate(new ConfigurationConventionDelegate(context =>
            {
                configureDelegate(_context, context);
            }));
            return this;
        }
        IRocketWebHostBuilder IRocketWebHostBuilder.ConfigureServices(Action<IServiceCollection> configureServices)
        {
            if (configureServices == null)
            {
                throw new ArgumentNullException(nameof(configureServices));
            }

            Scanner.AppendDelegate(new ServiceConventionDelegate(context =>
            {
                configureServices(context.Services);
            }));
            return this;
        }

        IRocketWebHostBuilder IRocketWebHostBuilder.ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
        {
            if (configureServices == null)
            {
                throw new ArgumentNullException(nameof(configureServices));
            }

            Scanner.AppendDelegate(new ServiceConventionDelegate(context =>
            {
                configureServices(_context, context.Services);
            }));
            return this;
        }

        string IWebHostBuilder.GetSetting(string key)
        {
            return ((IRocketWebHostBuilder) this).GetSetting(key);
        }

        IWebHostBuilder IWebHostBuilder.UseSetting(string key, string value)
        {
            return ((IRocketWebHostBuilder)this).UseSetting(key, value);
        }

        string IRocketWebHostBuilder.GetSetting(string key) => _webHostBuilder.GetSetting(key);
        IRocketWebHostBuilder IRocketWebHostBuilder.UseSetting(string key, string value)
        {
            _webHostBuilder.UseSetting(key, value);
            return this;
        }

        IWebHostBuilder IRocketWebHostBuilder.AsWebHostBuilder() => this;

        IRocketWebHostBuilder IRocketSurgeryHostBuilder<IRocketWebHostBuilder>.PrependDelegate(Delegate @delegate)
        {
            Scanner.PrependDelegate(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        IRocketWebHostBuilder IRocketSurgeryHostBuilder<IRocketWebHostBuilder>.PrependConvention(IConvention convention)
        {
            Scanner.PrependConvention(convention ?? throw new ArgumentNullException(nameof(convention)));
            return this;
        }

        IRocketWebHostBuilder IRocketSurgeryHostBuilder<IRocketWebHostBuilder>.AppendDelegate(Delegate @delegate)
        {
            Scanner.AppendDelegate(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        IRocketWebHostBuilder IRocketSurgeryHostBuilder<IRocketWebHostBuilder>.AppendConvention(IConvention convention)
        {
            Scanner.AppendConvention(convention ?? throw new ArgumentNullException(nameof(convention)));
            return this;
        }

        IRocketWebHostBuilder IRocketSurgeryHostBuilder<IRocketWebHostBuilder>.ExceptConvention(Type type)
        {
            Scanner.ExceptConvention(type ?? throw new ArgumentNullException(nameof(type)));
            return this;
        }

        IRocketWebHostBuilder IRocketSurgeryHostBuilder<IRocketWebHostBuilder>.ExceptConvention(Assembly assembly)
        {
            Scanner.ExceptConvention(assembly ?? throw new ArgumentNullException(nameof(assembly)));
            return this;
        }
    }
}
