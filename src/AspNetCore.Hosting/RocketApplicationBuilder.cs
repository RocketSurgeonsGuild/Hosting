using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;

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
}
