using System;

namespace Aerie.ForegroundServices
{
    public class ForegroundServiceLifetimeOptions
    {
        public bool HandleProcessExit { get; set; } = false;

        public bool HandleConsoleCancelKeyPress { get; set; } = false;
    }
}
