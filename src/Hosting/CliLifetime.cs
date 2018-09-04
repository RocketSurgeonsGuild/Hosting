using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;
using Rocket.Surgery.Extensions.CommandLine;

namespace Rocket.Surgery.Hosting
{
    /// <summary>
    /// Listens for Ctrl+C or SIGTERM and initiates shutdown.
    /// </summary>
    public class CliLifetime : IHostLifetime, IDisposable
    {
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ICommandLineExecutor _commandLineExecutor;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConsoleLifetime _consoleLifetime;

        public CliLifetime(IOptions<ConsoleLifetimeOptions> options, IHostingEnvironment environment, IApplicationLifetime applicationLifetime, ICommandLineExecutor commandLineExecutor, IServiceProvider serviceProvider)
        {
            _applicationLifetime = applicationLifetime;
            _commandLineExecutor = commandLineExecutor;
            _serviceProvider = serviceProvider;
            _consoleLifetime = new ConsoleLifetime(options, environment, applicationLifetime);
        }

        public int Result { get; private set; }

        public async Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();

            if (_commandLineExecutor.Application.IsShowingInformation || !_commandLineExecutor.IsDefaultCommand)
            {
                if (_commandLineExecutor.Application.IsShowingInformation)
                {
                    Result = 0;
                }
                else if (!_commandLineExecutor.IsDefaultCommand)
                {
                    Result = _commandLineExecutor.Execute(_serviceProvider);
                }

                Task.Run(() => _applicationLifetime.StopApplication());
                return;
            }

            Result = _commandLineExecutor.Execute(_serviceProvider);
            if (Result == int.MinValue)
            {
                await _consoleLifetime.WaitForStartAsync(cancellationToken);
            }
            else
            {
                Task.Run(() => _applicationLifetime.StopApplication());
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            await _consoleLifetime.StopAsync(cancellationToken);
        }

        public void Dispose()
        {
            _consoleLifetime.Dispose();
        }
    }
}
