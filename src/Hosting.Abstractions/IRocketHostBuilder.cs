using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;

namespace Rocket.Surgery.Hosting
{
    public interface IRocketHostBuilder : IConventionHostBuilder, IHostBuilder
    {
        IHostBuilder Builder { get; }
    }
}
