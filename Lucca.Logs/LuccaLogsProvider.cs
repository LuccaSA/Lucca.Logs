using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using StackExchange.Exceptional;

namespace Lucca.Logs
{
    public sealed class LuccaLogsProvider : ILoggerProvider
    {
        private readonly IOptionsMonitor<LuccaLoggerOptions> _options;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDisposable _changeListener;

        public LuccaLogsProvider(IOptionsMonitor<LuccaLoggerOptions> options, IHttpContextAccessor httpContextAccessor)
        {
            _options = options;
            _httpContextAccessor = httpContextAccessor;

            _changeListener = options.OnChange((o, name) =>
                {
                    PropagateExceptionalOptions(o);
                });

            PropagateExceptionalOptions(_options.CurrentValue);
        }

        private static void PropagateExceptionalOptions(LuccaLoggerOptions options)
        {
            Exceptional.Configure(exceptionalSetting =>
            {
                exceptionalSetting.DefaultStore = options.GenerateExceptionalStore();
            });
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            LuccaLoggerOptions opt = _options.CurrentValue;
            LogManager.Configuration = opt.Nlog;

            return new LuccaLogger(categoryName, _httpContextAccessor, LogManager.GetLogger(categoryName), opt, string.Empty);
        }

        public void Dispose()
        {
            _changeListener?.Dispose();
        }
    }
}