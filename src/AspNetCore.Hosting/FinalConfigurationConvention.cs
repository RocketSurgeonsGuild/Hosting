using Microsoft.Extensions.Configuration;
using Rocket.Surgery.Extensions.Configuration;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public class FinalConfigurationConvention : IConfigurationConvention
    {
        private readonly string[] _arguments;

        public FinalConfigurationConvention(string[] arguments = null)
        {
            _arguments = arguments;
        }

        public void Register(IConfigurationConventionContext context)
        {
            context.AddEnvironmentVariables();

            if (_arguments != null)
            {
                context.AddCommandLine(_arguments);
            }
        }
    }
}