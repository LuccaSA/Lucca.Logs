using System;
using Lucca.Logs.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Exceptional;
#if NETCOREAPP2_1
using Microsoft.AspNetCore.Http;
#else
using Lucca.Logs.AspnetLegacy;
#endif

namespace Lucca.Logs.AspnetCore
{
    public static class LuccaLoggerExtensions
    {
        public static ILoggingBuilder AddLuccaLogs(this ILoggingBuilder loggingBuilder, IConfigurationSection config, Action<LuccaLoggerOptions> configureOptions = null)
        {
            loggingBuilder.Services.AddLuccaLogs(config, configureOptions);
            return loggingBuilder;
        }

        public static ILoggingBuilder AddLuccaLogs(this ILoggingBuilder loggingBuilder, Action<LuccaLoggerOptions> configureOptions, ErrorStore errorStore = null)
        {
            loggingBuilder.Services.AddLuccaLogs(configureOptions, errorStore);
            return loggingBuilder;
        }

        private static IServiceCollection AddLuccaLogs(this IServiceCollection services, IConfigurationSection config, Action<LuccaLoggerOptions> configureOptions = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (!config.Exists())
            {
                throw new LogConfigurationException("Missing configuration section");
            }
            services.AddOptions();
#if NETCOREAPP2_1
            services.AddExceptional();
#endif
            services.Configure<LuccaLoggerOptions>(config);
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            services.RegisterLuccaLogsProvider();
            return services;
        }

        private static IServiceCollection AddLuccaLogs(this IServiceCollection services, Action<LuccaLoggerOptions> configureOptions, ErrorStore errorStore = null)
        {
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
#if NETCOREAPP2_1
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

            services.RegisterLuccaLogsProvider();
            return services;
        }

        private static void RegisterLuccaLogsProvider(this IServiceCollection services)
        {
#if NETCOREAPP2_1
            services.TryAddSingleton<IExceptionQualifier, GenericExceptionQualifier>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IHttpContextParser, HttpContextParserCore>();
            services.AddSingleton<ILoggerProvider, LuccaLogsProvider>();
            services.AddSingleton<IExceptionalWrapper, ExceptionalWrapperCore>();
#else
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessorLegacy>();
            services.TryAddSingleton<IHttpContextParser, HttpContextParserLegacy>();
            services.AddSingleton<ILoggerProvider, LuccaLogsProvider>();
            services.AddSingleton<IExceptionalWrapper, ExceptionalWrapperLegacy>();
#endif
        }
    }
}