using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Config;
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

            RegisterProvider(services);
            return services;
        }

        private static IServiceCollection AddLuccaLogs(this IServiceCollection services, Action<LuccaLoggerOptions> configureOptions, ErrorStore errorStore = null)
        {
            services.AddOptions();
            services.Configure(configureOptions);
            RegisterProvider(services);

            var provider = services.BuildServiceProvider();
            var opt = provider.GetService<IOptions<LuccaLoggerOptions>>();
            services.Configure<ExceptionalSettings>(o =>
            {

                o.DefaultStore = errorStore ?? opt.Value.GenerateExceptionalStore();
            });

            return services;
        }

        public static IApplicationBuilder UseLuccaLogs(this IApplicationBuilder app)
        {
            return app.UseMiddleware<EnableRequestContentRewindMiddleware>();
        }


        private static void RegisterProvider(IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ILoggerProvider, LuccaLogsProvider>();
        }
    }

    /// <summary>
    /// Middleware to enable HttpRequest Body content inspection
    /// </summary>
    public class EnableRequestContentRewindMiddleware
    {
        private readonly RequestDelegate _next;

        public EnableRequestContentRewindMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableRewind();
            await _next(context);
        }
    }

}