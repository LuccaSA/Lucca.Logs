using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lucca.Logs.Netcore.Tests
{
    public static class TestHelper
    {
        public static ServiceProvider Register<T>(Action<ILoggingBuilder> configure)
            where T : class
        {
            var services = new ServiceCollection();
            services.AddTransient<T>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddLogging(configure);
            return services.BuildServiceProvider();
        }
    }
}