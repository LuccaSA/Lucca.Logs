using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Targets;
using Xunit;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lucca.Logs.Tests
{
    public class DependencyTest
    {
        private readonly LuccaLoggerOptions _loggerOptions;

        public DependencyTest()
        {
            var target = new MemoryTarget
            {
                Name = "inmemory",
                Layout = "${message}|${level}|${exception}|${event-properties:EventId}"
            };
            _loggerOptions = new LuccaLoggerOptions();
            var nlogConf = new LoggingConfiguration();
            nlogConf.AddTarget(target);
            var networkRule = new LoggingRule("*", NLog.LogLevel.Trace, target);
            nlogConf.LoggingRules.Add(networkRule);

            _loggerOptions.NlogLoggingConfiguration = nlogConf;
        }

        [Theory]
        //[InlineData(LogLevel.Debug)]
        //[InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        public void FactoryTest(LogLevel logLevel)
        {
            ServiceProvider provider = Register<DummyLogFactoryPlayer>();
            var player = provider.GetRequiredService<DummyLogFactoryPlayer>();
            MemoryTarget target = GetTarget();

            player.Log(logLevel, 42, new Exception(), "the answer");
           
            string expected = String.Format("the answer|{0}|Exception of type 'System.Exception' was thrown.|42", logLevel.ToNLogLevel());
            Assert.Equal(expected, target.Logs.FirstOrDefault());
        }

        [Theory]
        //[InlineData(LogLevel.Debug)]
        //[InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        public void LoggerTest(LogLevel logLevel)
        {
            ServiceProvider provider = Register<DummyLogPlayer>();
            var player = provider.GetRequiredService<DummyLogPlayer>();
            MemoryTarget target = GetTarget();

            player.Log(logLevel, 42, new Exception(), "the answer");

            string expected = String.Format("the answer|{0}|Exception of type 'System.Exception' was thrown.|42", logLevel.ToNLogLevel());
            Assert.Equal(expected, target.Logs.FirstOrDefault());
        }

        private ServiceProvider Register<T>()
            where T : class
        {
            var services = new ServiceCollection();
            services.AddTransient<T>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddLuccaLogs(_loggerOptions);
            });
            return services.BuildServiceProvider();
        }

        private MemoryTarget GetTarget()
            => _loggerOptions.NlogLoggingConfiguration.FindTargetByName<MemoryTarget>("inmemory");

        public class DummyLogPlayer : ILogPlayer<DummyLogPlayer>
        {
            public ILogger<DummyLogPlayer> Logger { get; }

            public DummyLogPlayer(ILogger<DummyLogPlayer> logger)
            {
                Logger = logger;
            }
        }

        public class DummyLogFactoryPlayer : ILogPlayer<DummyLogFactoryPlayer>
        {
            public ILogger<DummyLogFactoryPlayer> Logger { get; }

            public DummyLogFactoryPlayer(ILoggerFactory loggerFactory)
            {
                Logger = loggerFactory.CreateLogger<DummyLogFactoryPlayer>();
            }
        }

    }
}