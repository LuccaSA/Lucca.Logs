using System;
using System.Linq;
using Lucca.Logs.AspnetCore;
using Lucca.Logs.Shared;
using Microsoft.Extensions.DependencyInjection;
using NLog.Config;
using NLog.Targets;
using Xunit;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lucca.Logs.Tests
{
    public class NLogInjectionTest
    {
        [Theory]
        //[InlineData(LogLevel.Debug)]
        //[InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        public void FactoryTest(LogLevel logLevel)
        {
            MemoryTarget target = null; 
            ServiceProvider provider = TestHelper.Register<DummyLogFactoryPlayer>(loggingBuilder =>
            {
                loggingBuilder.AddLuccaLogs(o =>
                {
                    target = BootStrapNLogInMemoryOption(o);
                }, "myLogger");
            });
            var player = provider.GetRequiredService<DummyLogFactoryPlayer>();

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
            MemoryTarget target = null;
            ServiceProvider provider = TestHelper.Register<DummyLogPlayer>(loggingBuilder =>
            {
                loggingBuilder.AddLuccaLogs(o =>
                {
                    target = BootStrapNLogInMemoryOption(o);
                }, "myLogger");
            });
            var player = provider.GetRequiredService<DummyLogPlayer>(); 

            player.Log(logLevel, 42, new Exception(), "the answer");

            string expected = String.Format("the answer|{0}|Exception of type 'System.Exception' was thrown.|42", logLevel.ToNLogLevel());
            Assert.Equal(expected, target.Logs.FirstOrDefault());
        }

        private static MemoryTarget BootStrapNLogInMemoryOption(LuccaLoggerOptions loggerOptions)
        {
            var target = new MemoryTarget
            {
                Name = "inmemory",
                Layout = "${message}|${level}|${exception}|${event-properties:EventId}"
            }; 
            var nlogConf = new LoggingConfiguration();
            nlogConf.AddTarget(target);
            var networkRule = new LoggingRule("*", NLog.LogLevel.Trace, target);
            nlogConf.LoggingRules.Add(networkRule);

            loggerOptions.Nlog = nlogConf;
            return target;
        }
    }
}