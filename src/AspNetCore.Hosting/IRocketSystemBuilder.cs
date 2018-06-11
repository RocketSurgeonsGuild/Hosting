using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public interface IRocketSystemBuilder : IApplicationBuilder
    {
        IConfiguration Configuration { get; }
    }
}