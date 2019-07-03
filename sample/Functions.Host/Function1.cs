using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Hosting.Functions;

[assembly: WebJobsStartup(typeof(Functions.Host.Startup))]

namespace Functions.Host
{
    public class Startup : IWebJobsStartup, IServiceConvention
    {
        public void Register(IServiceConventionContext context)
        {
            context.Services.AddTransient<ServiceA>();
        }

        public void Configure(IWebJobsBuilder builder)
        {
            builder.UseRocketSurgery(
                this,
                hostBuilder =>
                {
                    
                });
        }
    }

    public class ServiceA
    {

    }

    public class Function1
    {
        private readonly ServiceA _service;

        public Function1(ServiceA service)
        {
            _service = service;
        }

        [FunctionName("Function1")]
        public void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
