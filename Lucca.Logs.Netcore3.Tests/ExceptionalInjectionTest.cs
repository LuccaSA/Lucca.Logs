using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucca.Logs.AspnetCore;
using Lucca.Logs.Shared;
using Lucca.Logs.Shared.Exceptional;
using Lucca.Logs.Shared.Opserver;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Events; 
using Xunit;

namespace Lucca.Logs.Netcore.Tests
{
    public class ExceptionalInjectionTest
    {
        private readonly LogStoreInMemory _memoryStore;

        public ExceptionalInjectionTest()
        {
            _memoryStore = new LogStoreInMemory();
        }

        [Theory]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        public void FactoryTest(LogLevel logLevel)
        {
            ServiceProvider provider = TestHelper.Register<DummyLogFactoryPlayer>(loggingBuilder =>
            {
                //Exceptional.Settings.DefaultStore = _memoryStore;
                loggingBuilder.AddLuccaLogs(o =>
                {
                }, "myLogger",_memoryStore);
            });

            var player = provider.GetRequiredService<DummyLogFactoryPlayer>();
            //Assert.Equal(_memoryStore, Exceptional.Settings.DefaultStore);

            player.Log(logLevel, 42, new Exception(), "the answer");

            var found = _memoryStore.LogEvents
                .Select(e => e.ToExceptionalError())
                .Where(e => e != null)
                .ToList();

            if (logLevel > LogLevel.Debug)
            {
                Assert.Single(found);
                Assert.Equal("Exception of type 'System.Exception' was thrown.", found.FirstOrDefault()?.Message);
            }
            else
            {
                Assert.Empty(found);
            }
        }

    }

    public class LogStoreInMemory : LogStore
    {
        public Queue<LogEvent> LogEvents { get; set; } = new Queue<LogEvent>();

        public override bool TryWrite(LogEvent item)
        {
            LogEvents.Enqueue(item);
            return base.TryWrite(item);
        }

        public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        {
            return default;
        }

        public override ValueTask<LogEvent> ReadAsync(CancellationToken cancellationToken = default)
        {
            return new ValueTask<LogEvent>(LogEvents.Dequeue());
        }

    }
}