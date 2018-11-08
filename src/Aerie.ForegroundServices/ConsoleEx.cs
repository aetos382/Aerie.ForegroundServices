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
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

            Task.Run(() => {

                try
                {
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
