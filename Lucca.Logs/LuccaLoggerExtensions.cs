using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Exceptional;

namespace Lucca.Logs
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
            if (!config.Exists())
            {
                throw new LogConfigurationException("Missing configuration section");
            }
            services.AddOptions();
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
                services.Configure(configureOptions);
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
            services.AddSingleton<ILoggerProvider, LuccaLogsProvider>();
        }
    }
}