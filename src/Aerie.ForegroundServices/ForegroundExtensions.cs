using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using JetBrains.Annotations;

namespace Aerie.ForegroundServices
{
    public static class ForegroundExtensions
    {
        [NotNull]
        public static IHostBuilder UseForegroundLifetime(
            [NotNull] this IHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ConfigureServices(services => {
                services.AddSingleton<IHostLifetime, ForegroundLifetime>();
            });

            return builder;
        }

        [NotNull]
        public static IHostBuilder UseForegroundLifetime<TForegroundService>(
            [NotNull] this IHostBuilder builder)
            where TForegroundService : class, IForegroundService
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return UseForegroundLifetime<TForegroundService>(builder, null);
        }

        [NotNull]
        public static IHostBuilder UseForegroundLifetime<TForegroundService>(
            [NotNull] this IHostBuilder builder,
            [CanBeNull] ForegroundServiceLifetimeOptions options)
            where TForegroundService : class, IForegroundService
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ConfigureServices(services => {
                AddForegroundService<TForegroundService>(services);

                if (options != null)
                {
                    services.AddSingleton(Options.Create(options));
                }
            });

            return builder;
        }

        [NotNull]
        public static IServiceCollection AddForegroundService<TForegroundService>(
            [NotNull] this IServiceCollection services)
            where TForegroundService : class, IForegroundService
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IHostLifetime, ForegroundLifetime>();
            services.AddSingleton<IForegroundService, TForegroundService>();

            return services;
        }

        [NotNull]
        public static IServiceCollection AddForegroundService<TForegroundService>(
            [NotNull] this IServiceCollection services,
            [NotNull] Func<IServiceProvider, TForegroundService> implementationFactory)
            where TForegroundService : class, IForegroundService
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (implementationFactory == null)
            {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            services.AddSingleton<IHostLifetime, ForegroundLifetime>();
            services.AddSingleton<IForegroundService, TForegroundService>(implementationFactory);

            return services;
        }
    }
}
