using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Rocket.Surgery.Builders;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public class RocketApplicationBuilder : IRocketApplicationBuilder
    {
        private readonly IApplicationBuilder _applicationBuilder;

        public RocketApplicationBuilder(IApplicationBuilder applicationBuilder, IConfiguration configuration)
        {
            _applicationBuilder = applicationBuilder;
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IConfiguration Configuration { get; }

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            return _applicationBuilder.Use(middleware);
        }

        public IApplicationBuilder New()
        {
            return _applicationBuilder.New();
        }

        public RequestDelegate Build()
        {
            return _applicationBuilder.Build();
        }

        public IServiceProvider ApplicationServices
        {
            get => _applicationBuilder.ApplicationServices;
            set => _applicationBuilder.ApplicationServices = value;
        }

        public IFeatureCollection ServerFeatures => _applicationBuilder.ServerFeatures;

        public IDictionary<string, object> Properties => _applicationBuilder.Properties;
    }


    public abstract class RocketApplicationBuilder<TBuilder> : Builder<TBuilder>, IRocketApplicationBuilder
        where TBuilder : class, IRocketApplicationBuilder
    {
        private readonly IRocketApplicationBuilder _app;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBuilder{TBuilder}" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="app"></param>
        protected RocketApplicationBuilder(TBuilder parent, IRocketApplicationBuilder app) : base(parent, new Dictionary<object, object>()) => _app = app ?? throw new ArgumentNullException(nameof(app));

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        /// TODO Edit XML Comment Template for Configuration
        public IConfiguration Configuration => Parent.Configuration;

        /// <summary>
        /// Adds a middleware delegate to the application's request pipeline.
        /// </summary>
        /// <param name="middleware">The middleware delgate.</param>
        /// <returns>The <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" />.</returns>
        /// TODO Edit XML Comment Template for Use
        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            return _app.Use(middleware);
        }

        IApplicationBuilder IApplicationBuilder.New()
        {
            return _app.New();
        }

        RequestDelegate IApplicationBuilder.Build()
        {
            return _app.Build();
        }

        /// <summary>
        /// Gets or sets the <see cref="T:_System.IServiceProvider" /> that provides access to the application's service container.
        /// </summary>
        /// <value>The application services.</value>
        /// TODO Edit XML Comment Template for ApplicationServices
        public IServiceProvider ApplicationServices
        {
            get => _app.ApplicationServices;
            set => _app.ApplicationServices = value;
        }

        /// <summary>
        /// Gets the set of HTTP features the application's server provides.
        /// </summary>
        /// <value>The server features.</value>
        /// TODO Edit XML Comment Template for ServerFeatures
        public IFeatureCollection ServerFeatures => _app.ServerFeatures;

        /// <summary>
        /// Gets a key/value collection that can be used to share data between middleware.
        /// </summary>
        /// <value>The properties.</value>
        /// TODO Edit XML Comment Template for Properties
        public new IDictionary<string, object> Properties => _app.Properties;
    }
}
