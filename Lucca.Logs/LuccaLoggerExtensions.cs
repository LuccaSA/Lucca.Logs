using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Lucca.Logs
{
	public static class LuccaLoggerExtensions
	{
		public static ILoggingBuilder AddLuccaLogs(this ILoggingBuilder builder, Action<LuccaLoggerOptions> configure)
		{
            RegisterProvider(builder);
            builder.Services.Configure(configure);
			return builder;
		}

		public static ILoggingBuilder AddLuccaLogs(this ILoggingBuilder builder, LuccaLoggerOptions options = null)
        {
            RegisterProvider(builder);
            builder.Services.AddSingleton(options ?? LuccaLoggerOptions.Default);
            return builder;
        }

        private static void RegisterProvider(ILoggingBuilder builder)
        {
            builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddSingleton<ILoggerProvider, LuccaLogsProvider>();
        }
    }
}