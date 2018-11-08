using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aerie.ForegroundServices
{
    public static class ConsoleEx
    {
        public static Task<ConsoleKeyInfo> ReadKeyAsync(
            bool intercept = false,
            CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<ConsoleKeyInfo>();

            Task.Run(() => {

                try
                {
                    SpinWait.SpinUntil(() => Console.KeyAvailable || cancellationToken.IsCancellationRequested);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        tcs.TrySetCanceled(cancellationToken);
                        return;
                    }

                    var key = Console.ReadKey(intercept);
                    tcs.TrySetResult(key);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }

            }, cancellationToken);

            return tcs.Task;
        }
    }
}
