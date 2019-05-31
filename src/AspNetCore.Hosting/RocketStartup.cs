using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Reflection.Extensions;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public abstract class RocketStartup
    {
        private readonly IRocketWebHostingContext _context;
        private readonly IRocketServiceComposer _serviceComposer;
        protected string SystemPath = "/system";

        protected RocketStartup(
            IRocketWebHostingContext context,
            IRocketServiceComposer serviceComposer,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            _context = context;
            _serviceComposer = serviceComposer;
            Environment = environment;
            Configuration = configuration;
            if (this is IConvention convention)
            {
                context.Scanner.AppendConvention(convention);
            }
        }

        public IHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
    }
}
