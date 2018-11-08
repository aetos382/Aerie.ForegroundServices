using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBrains.Annotations;

namespace Aerie.ForegroundServices
{
    internal class MainService :
        IForegroundService
    {
        [NotNull]
        private readonly ILogger _logger;

        public MainService(
            [NotNull] ILogger<MainService> logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this._logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("MainService Started.");

            Console.WriteLine("Press any key to exit.");

            // Do not wait synchronously !! (for example by Console.ReadKey)
            await ConsoleEx.ReadKeyAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            this._logger.LogInformation("Shutting down...");
        }
    }
}
