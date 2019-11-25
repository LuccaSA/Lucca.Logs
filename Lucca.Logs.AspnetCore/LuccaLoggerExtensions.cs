using System;
using Lucca.Logs.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Exceptional;
#if NETCOREAPP2_1 || NETCOREAPP3_0
using Microsoft.AspNetCore.Http;
#else
using Lucca.Logs.AspnetLegacy;
#endif

namespace Lucca.Logs.AspnetCore
{
    public static class LuccaLoggerExtensions
    {
        public static ILoggingBuilder AddLuccaLogs(this ILoggingBuilder loggingBuilder,
            IConfigurationSection config, string appName, Action<LuccaLoggerOptions> configureOptions = null)
        {
            loggingBuilder.Services.AddLuccaLogs(config, appName, configureOptions);
            return loggingBuilder;
        }

        public static ILoggingBuilder AddLuccaLogs(this ILoggingBuilder loggingBuilder,
            Action<LuccaLoggerOptions> configureOptions, string appName, ErrorStore errorStore = null)
        {
            loggingBuilder.Services.AddLuccaLogs(configureOptions, appName, errorStore);
            return loggingBuilder;
        }

        private static IServiceCollection AddLuccaLogs(this IServiceCollection services, IConfigurationSection config,
            string appName, Action<LuccaLoggerOptions> configureOptions = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (string.IsNullOrWhiteSpace(appName))
            {
                throw new ArgumentNullException(nameof(appName));
            }

            if (!config.Exists())
            {
                throw new LogConfigurationException("Missing configuration section");
            }
            services.AddOptions();
            services.Configure<LuccaLoggerOptions>(config);
#if NETCOREAPP2_1 || NETCOREAPP3_0
            services.AddExceptional(e =>
            {
                var luccaLogsOption = config.Get<LuccaLoggerOptions>();
                e.DefaultStore = luccaLogsOption.GenerateExceptionalStore();
            });
#endif
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            services.PostConfigure<LuccaLoggerOptions>(o =>
            {
                if (string.IsNullOrWhiteSpace(o.ApplicationName))
                {
                    o.ApplicationName = appName;
                }
            });
            services.RegisterLuccaLogsProvider();
            return services;
        }

        private static IServiceCollection AddLuccaLogs(this IServiceCollection services,
            Action<LuccaLoggerOptions> configureOptions, string appName, ErrorStore errorStore = null)
        {
            if (string.IsNullOrWhiteSpace(appName))
            {
                throw new ArgumentNullException(nameof(appName));
            }

            services.AddOptions();

            if (configureOptions != null)
            {
                services.Configure<LuccaLoggerOptions>(o =>
                {
                    o.ExplicitErrorStore = errorStore;
                    configureOptions(o);
                });
            }

            if (errorStore != null)
            {
#if NETCOREAPP2_1 || NETCOREAPP3_0
                services.AddExceptional(o =>
                {
                    o.Store.Type = errorStore.GetType().ToString();
                    o.DefaultStore = errorStore;
                });
#else
                Exceptional.Configure(o =>
                {
                    o.Store.Type = errorStore.GetType().ToString();
                    o.DefaultStore = errorStore;
                });
#endif
            }
            services.PostConfigure<LuccaLoggerOptions>(o =>
            {
                if (string.IsNullOrWhiteSpace(o.ApplicationName))
                {
                    o.ApplicationName = appName;
                }
            });
            services.RegisterLuccaLogsProvider();
            return services;
        }

        private static void RegisterLuccaLogsProvider(this IServiceCollection services)
        {
#if NETCOREAPP2_1 || NETCOREAPP3_0
            services.TryAddSingleton<IExceptionQualifier, GenericExceptionQualifier>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IContextParser, HttpContextParserCore>();
            services.AddSingleton<ILoggerProvider, LuccaLogsProvider>();
            services.AddSingleton<IExceptionalWrapper, ExceptionalWrapperCore>();
#else
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessorLegacy>();
            services.TryAddSingleton<IContextParser, HttpContextParserLegacy>();
            services.AddSingleton<ILoggerProvider, LuccaLogsProvider>();
            services.AddSingleton<IExceptionalWrapper, ExceptionalWrapperLegacy>();
#endif
        }
    }
}