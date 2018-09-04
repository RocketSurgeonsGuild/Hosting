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

        public IRocketWebHostBuilder ConfigureAppConfiguration(Action<WebHostBuilderContext, IMsftConfigurationBuilder> configureDelegate)
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
        public IRocketWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            if (configureServices == null)
            {
                throw new ArgumentNullException(nameof(configureServices));
            }

            _webHostBuilder.ConfigureServices(configureServices);
            return this;
        }

        public IRocketWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
        {
            if (configureServices == null)
            {
                throw new ArgumentNullException(nameof(configureServices));
            }

            _webHostBuilder.ConfigureServices(configureServices);
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

        public string GetSetting(string key) => _webHostBuilder.GetSetting(key);
        public IRocketWebHostBuilder UseSetting(string key, string value)
        {
            _webHostBuilder.UseSetting(key, value);
            return this;
        }

        public IWebHostBuilder AsWebHostBuilder() => this;

        public IRocketWebHostBuilder PrependDelegate(Delegate @delegate)
        {
            Scanner.PrependDelegate(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        public IRocketWebHostBuilder PrependConvention(IConvention convention)
        {
            Scanner.PrependConvention(convention ?? throw new ArgumentNullException(nameof(convention)));
            return this;
        }

        public IRocketWebHostBuilder AppendDelegate(Delegate @delegate)
        {
            Scanner.AppendDelegate(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        public IRocketWebHostBuilder AppendConvention(IConvention convention)
        {
            Scanner.AppendConvention(convention ?? throw new ArgumentNullException(nameof(convention)));
            return this;
        }

        public IRocketWebHostBuilder ExceptConvention(Type type)
        {
            Scanner.ExceptConvention(type ?? throw new ArgumentNullException(nameof(type)));
            return this;
        }

        public IRocketWebHostBuilder ExceptConvention(Assembly assembly)
        {
            Scanner.ExceptConvention(assembly ?? throw new ArgumentNullException(nameof(assembly)));
            return this;
        }
    }
}
