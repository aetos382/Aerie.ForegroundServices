using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aerie.ForegroundServices
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureHostConfiguration(builder => {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    builder.AddJsonFile("hostsettings.json", optional: true);
                    builder.AddEnvironmentVariables(prefix: "FOREGROUNDAPP_");
                    builder.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((context, builder) => {
                    builder.AddJsonFile("appsettings.json", optional: true);
                    builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                    builder.AddEnvironmentVariables(prefix: "FOREGROUNDAPP_");
                    builder.AddCommandLine(args);
                })
                .ConfigureLogging((context, builder) => {
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .UseForegroundLifetime<MainService>()
                .Build();

            await host.RunAsync();
        }
    }
}
