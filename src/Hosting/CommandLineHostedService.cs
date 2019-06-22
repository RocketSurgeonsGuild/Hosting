using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.CommandLine;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting
{
    class CommandLineHostedService : IHostedService
    {
        private readonly ICommandLineExecutor _executor;
        private readonly IServiceProvider _serviceProvider;
#if NETCOREAPP3_0
        private readonly IHostApplicationLifetime _lifetime;
#else
        private readonly IApplicationLifetime _lifetime;
#endif
        private readonly CommandLineResult _result;
        private readonly bool _isWebApp;
        private readonly ILogger<CommandLineHostedService> _logger;

        public CommandLineHostedService(
            IServiceProvider serviceProvider,
            ICommandLineExecutor executor,
#if NETCOREAPP3_0
            IHostApplicationLifetime lifetime,
#else
            IApplicationLifetime lifetime,
#endif
        CommandLineResult commandLineResult,
            bool isWebApp)
        {
            this._executor = executor ?? throw new ArgumentNullException(nameof(executor));
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
            this._result = commandLineResult ?? throw new ArgumentNullException(nameof(commandLineResult));
            this._isWebApp = isWebApp;
            _logger = _serviceProvider.GetRequiredService<ILogger<CommandLineHostedService>>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _lifetime.ApplicationStarted.Register(() =>
            {
                if (!(_executor.IsDefaultCommand && _isWebApp))
                {
                    try
                    {
                        _result.Value = _executor.Execute(_serviceProvider);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Command failed to execute");
                        _result.Value = -1;
                    }
                    finally
                    {
                        _lifetime.StopApplication();
                    }
                }
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
