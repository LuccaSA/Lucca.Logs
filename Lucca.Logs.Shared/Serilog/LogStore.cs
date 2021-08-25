using Serilog.Events;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Lucca.Logs.Shared
{
    public class LogStore
    {
        private readonly Channel<LogEvent> _channel;
        public LogStore()
        {
            _channel = Channel.CreateUnbounded<LogEvent>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleWriter = false,
                SingleReader = false
            });
        }

        public virtual bool TryWrite(LogEvent item)
        {
            return _channel.Writer.TryWrite(item);
        }

        public virtual ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.WaitToReadAsync(cancellationToken);
        }

        public virtual ValueTask<LogEvent> ReadAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.ReadAsync(cancellationToken);
        }

        public virtual void Complete(Exception error = null)
        {
            _channel.Writer.Complete(error);
        }
    }
}