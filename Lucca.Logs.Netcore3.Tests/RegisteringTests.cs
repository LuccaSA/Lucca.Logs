using Lucca.Logs.AspnetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using System;
using System.Linq;
using Xunit;

namespace Lucca.Logs.Netcore.Tests
{
    public class RegisteringTests
    {
        [Theory]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        public void FactoryTest(LogLevel logLevel)
        {
            var targetSink = new TestSink();
            ServiceProvider provider = TestHelper.Register<DummyLogFactoryPlayer>(loggingBuilder =>
            {
                loggingBuilder.AddLuccaLogs(o =>
                {
                    o.TestSink = targetSink;
                }, "myLogger");
            });
            var player = provider.GetRequiredService<DummyLogFactoryPlayer>();
             
            player.Log(logLevel, 42, new Exception(), "the answer");
            
            var logEvent = targetSink.Writes.FirstOrDefault(); 
            Assert.Equal("the answer", logEvent.MessageTemplate.Text);
            Assert.Equal(logLevel, LevelConvert.ToExtensionsLevel(logEvent.Level));
        }

        [Theory]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        public void LoggerTest(LogLevel logLevel)
        {
            var targetSink = new TestSink();
            ServiceProvider provider = TestHelper.Register<DummyLogPlayer>(loggingBuilder =>
            {
                loggingBuilder.AddLuccaLogs(o =>
                {
                    o.TestSink = targetSink;
                }, "myLogger");
            });
            var player = provider.GetRequiredService<DummyLogPlayer>();

            player.Log(logLevel, 42, new Exception(), "the answer");

            var logEvent = targetSink.Writes.FirstOrDefault();
            Assert.Equal("the answer", logEvent.MessageTemplate.Text);
            Assert.Equal(logLevel, LevelConvert.ToExtensionsLevel(logEvent.Level));
            Assert.Equal("{ Id: 42 }", logEvent.Properties["EventId"].ToString());
        }
    }
}