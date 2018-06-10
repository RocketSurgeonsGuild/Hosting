using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public abstract class RocketApplicationBuilder<TBuilder> : IRocketApplicationBuilder
        where TBuilder : class, IRocketApplicationBuilder
    {
        private readonly TBuilder _parent;
        private readonly IRocketApplicationBuilder _app;

        protected RocketApplicationBuilder(TBuilder parent, IRocketApplicationBuilder app)
        {
            _parent = parent;
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        public IConfiguration Configuration => _parent.Configuration;

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            return _app.Use((Func<RequestDelegate, RequestDelegate>) middleware);
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
