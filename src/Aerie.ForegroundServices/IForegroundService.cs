using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aerie.ForegroundServices
{
    public interface IForegroundService
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
