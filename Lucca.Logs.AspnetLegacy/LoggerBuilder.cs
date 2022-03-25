using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lucca.Logs.AspnetLegacy
{
    public static class LoggerBuilder
    {
        public static ILoggerFactory CreateLuccaLogsFactory(this IServiceCollection serviceCollection, Action<ILoggingBuilder> configure)
        {
            return serviceCollection
                .AddLogging(configure)
                .BuildServiceProvider()
                .GetRequiredService<ILoggerFactory>();
        }

        private static readonly ServiceCollection _serviceCollection = new();
   
        public static ILoggerFactory CreateLuccaLogsFactory(Action<ILoggingBuilder> configure)
        {
            return _serviceCollection.CreateLuccaLogsFactory(configure);
        }
    }
}