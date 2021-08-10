using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;

namespace Lucca.Logs.Netcore.Tests
{
    public class TestSink : ILogEventSink
    {
        public List<LogEvent> Writes { get; set; } = new List<LogEvent>();

        public void Emit(LogEvent logEvent)
        {
            Writes.Add(logEvent);
        }
    }
}