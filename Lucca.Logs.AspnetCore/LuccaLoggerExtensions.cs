using System;
using Lucca.Logs.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Exceptional;

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
            services.AddExceptional();
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

            if(errorStore != null)
            {
                services.AddExceptional(o =>
                {
                    o.Store.Type = errorStore.GetType().ToString();
                    o.DefaultStore = errorStore;
                });
            }

            services.RegisterLuccaLogsProvider(); 
            return services;
        }

        public static IApplicationBuilder UseLuccaLogs(this IApplicationBuilder app, bool enableContentLog = true)
        {
            var builder = app;
            if (enableContentLog)
            {
                builder = builder.UseMiddleware<EnableRequestContentRewindMiddleware>();
            }
            return builder;
        }

        private static void RegisterLuccaLogsProvider(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IHttpContextWrapper, HttpContextCore>();
            services.AddSingleton<ILoggerProvider, LuccaLogsProvider>();
        }
    }
}