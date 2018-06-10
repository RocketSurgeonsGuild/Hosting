using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public class RocketApplicationBuilder : IRocketApplicationBuilder
    {
        private readonly IApplicationBuilder _app;

        public RocketApplicationBuilder(IApplicationBuilder app, IConfiguration configuration)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IConfiguration Configuration { get; }

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

        public IServiceProvider ApplicationServices
        {
            get => _app.ApplicationServices;
            set => _app.ApplicationServices = value;
        }

        public IFeatureCollection ServerFeatures => _app.ServerFeatures;

        public IDictionary<string, object> Properties => _app.Properties;
    }
}
