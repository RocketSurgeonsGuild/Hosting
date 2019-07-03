using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.CommandLine;

namespace Rocket.Surgery.Hosting
{
    /// <summary>
    /// Class CommandLineHostedService.
    /// Implements the <see cref="IHostedService" />
    /// </summary>
    /// <seealso cref="IHostedService" />
    class CommandLineHostedService : IHostedService
    {
        private readonly ICommandLineExecutor _executor;
        private readonly IServiceProvider _serviceProvider;
#if NETSTANDARD2_0 || NETCOREAPP2_1
        private readonly IApplicationLifetime _lifetime;
#else
        private readonly IHostApplicationLifetime _lifetime;
#endif
        private readonly CommandLineResult _result;
        private readonly bool _isWebApp;
        private readonly ILogger<CommandLineHostedService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineHostedService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="executor">The executor.</param>
        /// <param name="lifetime">The lifetime.</param>
        /// <param name="commandLineResult">The command line result.</param>
        /// <param name="isWebApp">if set to <c>true</c> [is web application].</param>
        /// <exception cref="ArgumentNullException">
        /// executor
        /// or
        /// serviceProvider
        /// or
        /// lifetime
        /// or
        /// commandLineResult
        /// </exception>
        public CommandLineHostedService(
            IServiceProvider serviceProvider,
            ICommandLineExecutor executor,
#if NETSTANDARD2_0 || NETCOREAPP2_1
            IApplicationLifetime lifetime,
#else
            IHostApplicationLifetime lifetime,
#endif
        CommandLineResult commandLineResult,
            bool isWebApp)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
            _result = commandLineResult ?? throw new ArgumentNullException(nameof(commandLineResult));
            _isWebApp = isWebApp;
            _logger = _serviceProvider.GetRequiredService<ILogger<CommandLineHostedService>>();
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns>Task.</returns>
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

        /// <summary>
        /// Stops the asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
