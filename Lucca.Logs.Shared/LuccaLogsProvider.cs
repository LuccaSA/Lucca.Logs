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
        private readonly IExceptionQualifier _filters;
        private readonly IExceptionalWrapper _exceptionalWrapper;
        private readonly IDisposable _changeListener;
        private readonly IEnumerable<ILogDetailsExtractor> _logDetailsExtractors;

        public LuccaLogsProvider(IOptionsMonitor<LuccaLoggerOptions> options, IHttpContextParser httpContextAccessor, IExceptionQualifier filters, IExceptionalWrapper exceptionalWrapper, IEnumerable<ILogDetailsExtractor> logDetailsExtractors)
        {
            _options = options;
            _httpContextAccessor = httpContextAccessor;
            _filters = filters;
            _exceptionalWrapper = exceptionalWrapper;
            _logDetailsExtractors = logDetailsExtractors;

            _changeListener = options.OnChange((o, name) => PropagateOptions(o));

            PropagateOptions(_options.CurrentValue);
        }

        private void PropagateOptions(LuccaLoggerOptions options)
        {
            LogManager.Configuration = options.Nlog;

        }

        
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            LuccaLoggerOptions opt = _options.CurrentValue;
            var logExtractor = new LogExtractor(_logDetailsExtractors, new EnvironmentDetailsExtractor(opt));
            return new LuccaLogger(categoryName, _httpContextAccessor, LogManager.GetLogger(categoryName), opt, logExtractor, _filters, _exceptionalWrapper, _options.CurrentValue.ApplicationName!);
        }

        public void Dispose()
        {
            _changeListener?.Dispose();
        }
    }
}
