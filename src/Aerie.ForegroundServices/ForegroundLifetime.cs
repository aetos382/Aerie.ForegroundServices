using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using JetBrains.Annotations;

namespace Aerie.ForegroundServices
{
    public sealed class ForegroundLifetime :
        IHostLifetime
    {
        [NotNull]
        private readonly IEnumerable<IForegroundService> _foregroundServices;

        [NotNull]
        private readonly ForegroundServiceLifetimeOptions _options;

        [NotNull]
        private readonly IApplicationLifetime _applicationLifetime;

        [NotNull]
        private readonly ILogger _logger;

        public ForegroundLifetime(
            [NotNull] IEnumerable<IForegroundService> foregroundServices,
            [NotNull] IOptions<ForegroundServiceLifetimeOptions> options,
            [NotNull] IApplicationLifetime applicationLifetime,
            [NotNull] ILogger<ForegroundLifetime> logger)
        {
            if (foregroundServices == null)
            {
                throw new ArgumentNullException(nameof(foregroundServices));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (applicationLifetime == null)
            {
                throw new ArgumentNullException(nameof(applicationLifetime));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this._foregroundServices = foregroundServices;
            this._options = options.Value;
            this._applicationLifetime = applicationLifetime;
            this._logger = logger;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            if (this._options.HandleProcessExit)
            {
                AppDomain.CurrentDomain.ProcessExit += (sender, e) => {
                    this._applicationLifetime.StopApplication();
                };
            }

            if (this._options.HandleConsoleCancelKeyPress)
            {
                Console.CancelKeyPress += (sender, e) => {
                    e.Cancel = true;
                    this._applicationLifetime.StopApplication();
                };
            }

            this._applicationLifetime.ApplicationStarted.Register(this.OnStart);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void OnStart()
        {
            var tasks = new List<Task>();

            foreach (var service in this._foregroundServices)
            {
                this._applicationLifetime.ApplicationStopping.ThrowIfCancellationRequested();

                try
                {
                    var task = service.RunAsync(this._applicationLifetime.ApplicationStopping);

                    task.ContinueWith(
                        t => this._logger.LogError(t.Exception, t.Exception.Message),
                        TaskContinuationOptions.OnlyOnFaulted);

                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, ex.Message);
                }
            }

            Task.WhenAll(tasks).ContinueWith(_ => this._applicationLifetime.StopApplication());
        }
    }
}
