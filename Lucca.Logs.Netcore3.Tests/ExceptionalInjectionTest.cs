using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucca.Logs.AspnetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Stores;
using Xunit;

namespace Lucca.Logs.Netcore.Tests
{
    public class ExceptionalInjectionTest
    {
        private readonly ErrorStore _memoryStore;

        public ExceptionalInjectionTest()
        {
            _memoryStore = new MemoryErrorStore(42);
        }

        [Theory]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        public async Task FactoryTest(LogLevel logLevel)
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

            List<Error> found = await _memoryStore.GetAllAsync();
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
}