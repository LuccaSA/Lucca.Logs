using System;
using Lucca.Logs.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using StackExchange.Exceptional;

namespace Lucca.Logs.AspnetCore
{
    public sealed class LuccaLogsProvider : ILoggerProvider
    {
        private readonly IOptionsMonitor<LuccaLoggerOptions> _options;
        private readonly IHttpContextWrapper _httpContextAccessor;
        private readonly IDisposable _changeListener;

        public LuccaLogsProvider(IOptionsMonitor<LuccaLoggerOptions> options, IHttpContextWrapper httpContextAccessor)
        {
            _options = options;
            _httpContextAccessor = httpContextAccessor;

            _changeListener = options.OnChange((o, name) =>
                {
                    PropagateOptions(o);
                });

            PropagateOptions(_options.CurrentValue);
        }

        private static void PropagateOptions(LuccaLoggerOptions options)
        {
            LogManager.Configuration = options.Nlog;
            Exceptional.Configure(exceptionalSetting =>
            { 
                exceptionalSetting.DefaultStore = options.GenerateExceptionalStore();
            });
        }
         
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            LuccaLoggerOptions opt = _options.CurrentValue;
            return new LuccaLogger(categoryName, _httpContextAccessor, LogManager.GetLogger(categoryName), opt, string.Empty);
        }

        public void Dispose()
        {
            _changeListener?.Dispose();
        }
    }
}