using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;

namespace Lucca.Logs.Shared
{
    public sealed class LuccaLogsProvider : ILoggerProvider
    {
        private readonly IOptionsMonitor<LuccaLoggerOptions> _options;
        private readonly IHttpContextParser _httpContextAccessor;
        private readonly IEnumerable<IExceptionalFilter> _filters;
        private readonly IExceptionalWrapper _exceptionalWrapper;
        private readonly IDisposable _changeListener;

        public LuccaLogsProvider(IOptionsMonitor<LuccaLoggerOptions> options, IHttpContextParser httpContextAccessor, IEnumerable<IExceptionalFilter> filters, IExceptionalWrapper exceptionalWrapper)
        {
            _options = options;
            _httpContextAccessor = httpContextAccessor;
            _filters = filters;
            _exceptionalWrapper = exceptionalWrapper;

            _changeListener = options.OnChange((o, name) =>
                {
                    PropagateOptions(o);
                });

            PropagateOptions(_options.CurrentValue);
        }

        private void PropagateOptions(LuccaLoggerOptions options)
        {
            LogManager.Configuration = options.Nlog;

            _exceptionalWrapper.Configure(exceptionalSetting =>
            {
                exceptionalSetting.DefaultStore = options.GenerateExceptionalStore();
            });
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            LuccaLoggerOptions opt = _options.CurrentValue;
            return new LuccaLogger(categoryName, _httpContextAccessor, LogManager.GetLogger(categoryName), opt, _filters, _exceptionalWrapper, categoryName);
        }

        public void Dispose()
        {
            _changeListener?.Dispose();
        }
    }
}