using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lucca.Logs.Shared
{
    internal sealed class BackgroundWorkerSink : ILogEventSink, IDisposable
    {
        private readonly ILogEventSink _wrappedSink;
        private readonly ILogEventSinkAsync _logEventSinkAsync;
        private readonly LogStore _store;
        private readonly Task _worker;

        public BackgroundWorkerSink(ILogEventSink wrappedSink, ILogEventSinkAsync logEventSinkAsync, LogStore store)
        {
            _wrappedSink = wrappedSink ?? throw new ArgumentNullException(nameof(wrappedSink));
            _logEventSinkAsync = logEventSinkAsync;
            _store = store ?? new LogStore();

            _worker = Task.Factory
                .StartNew(PumpAsync, CancellationToken.None, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default)
                .Unwrap();
        }

        public void Emit(LogEvent logEvent)
        {
            _store.TryWrite(logEvent);
        }

        public void Dispose()
        {
            // Prevent any more events from being added
            _store.Complete();

            // Allow queued events to be flushed
            _worker
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            (_wrappedSink as IDisposable)?.Dispose();
        }

        private async Task PumpAsync()
        {
            try
            {
                while (await _store.WaitToReadAsync())
                {
                    var logEvent = await _store.ReadAsync();
                    try
                    {
                        await _logEventSinkAsync.EmitAsync(logEvent);
                        // todo : clean logEvent.TryAdd(LogMeta.RawPostedData, logdetail.Payload);
                        _wrappedSink.Emit(logEvent);
                    }
                    catch (Exception ex)
                    {
                        SelfLog.WriteLine("{0} failed to emit event to wrapped sink: {1}", typeof(BackgroundWorkerSink), ex);
                    }
                }
            }
            catch (Exception fatal)
            {
                SelfLog.WriteLine("{0} fatal error in worker thread: {1}", typeof(BackgroundWorkerSink), fatal);
            }
        }
    }
}
