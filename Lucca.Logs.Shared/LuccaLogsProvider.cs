using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

            _exceptionalWrapper.Configure(exceptionalSetting =>
            {
                exceptionalSetting.DefaultStore = options.GenerateExceptionalStore();

                exceptionalSetting.LogFilters.Cookie["password"] = "***";
                exceptionalSetting.LogFilters.Header["password"] = "***";
                exceptionalSetting.LogFilters.Form["password"] = "***";
                exceptionalSetting.LogFilters.Header["password"] = "***";
                exceptionalSetting.LogFilters.QueryString["password"] = "***";

                exceptionalSetting.OnBeforeLog += (o, eb) =>
                {
                    string? querystring = eb?.Error?.ServerVariables?.Get("QUERY_STRING");
                    if (querystring is not null)
                    {
                        eb!.Error.ServerVariables.Set("QUERY_STRING", querystring.ClearQueryStringPassword());
                    }
                };
            });
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

    internal static class CleanExtension
    {
        private static readonly Regex _passwordClean = new("(?<=[?&]" + Regex.Escape("password") + "=)[^&]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string? ClearQueryStringPassword(this string? source)
        {
            if (source is null)
            {
                return null;
            }

            if (source.IndexOf("password", StringComparison.OrdinalIgnoreCase) > -1)
            {
               return  _passwordClean.Replace(source, "***");
            }

            return source;
        }
    }
}
