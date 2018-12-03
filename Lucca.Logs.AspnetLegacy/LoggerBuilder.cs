using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lucca.Logs.AspnetLegacy
{
    /// <summary>
    /// methods for setting up logging services in an IServiceCollection
    /// </summary>
    public static class LoggerBuilder
    {
        /// <summary>
        /// Adds logging services to the specified IServiceCollection
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection to which the configuration will be applied</param>
        /// <param name="configure">The ILoggingBuilder configuration delegate</param>
        /// <returns></returns>
        public static ILoggerFactory CreateLuccaLogsFactory(this IServiceCollection serviceCollection, Action<ILoggingBuilder> configure)
        {
            return serviceCollection
                .AddLogging(configure)
                .BuildServiceProvider()
                .GetRequiredService<ILoggerFactory>();
        }

        private static readonly ServiceCollection _serviceCollection = new ServiceCollection();

        /// <summary>
        /// Adds logging services to new IServiceCollection
        /// </summary>
        /// <param name="configure">The ILoggingBuilder configuration delegate</param>
        /// <returns></returns>
        public static ILoggerFactory CreateLuccaLogsFactory(Action<ILoggingBuilder> configure)
        {
            return _serviceCollection.CreateLuccaLogsFactory(configure);
        }
    }
}