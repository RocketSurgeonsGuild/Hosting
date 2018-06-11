using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public interface IRocketApplicationBuilder : IApplicationBuilder
    {
        IConfiguration Configuration { get; }
    }
}
