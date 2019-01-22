using System;
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

        public LuccaLogsProvider(IOptionsMonitor<LuccaLoggerOptions> options, IHttpContextParser httpContextAccessor, IExceptionQualifier filters, IExceptionalWrapper exceptionalWrapper)
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

                exceptionalSetting.LogFilters.Cookie["password"] = "***";
                exceptionalSetting.LogFilters.Header["password"] = "***";
                exceptionalSetting.LogFilters.Form["password"] = "***";
                exceptionalSetting.LogFilters.Header["password"] = "***";
                exceptionalSetting.LogFilters.QueryString["password"] = "***";

                exceptionalSetting.OnBeforeLog += (o, eb) =>
                {
                    foreach (var sv in eb.Error.ServerVariablesSerializable)
                    {
                        if (sv.Name.Equals("QUERY_STRING", StringComparison.OrdinalIgnoreCase))
                        {
                            sv.Value = sv.Value.ClearQueryStringPassword();
                        }
                    }
                };
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

    internal static class CleanExtension
    {
        private static readonly Regex _passwordClean = new Regex("(?<=[?&]" + Regex.Escape("password") + "=)[^&]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string ClearQueryStringPassword(this string source)
        {
            if (source == null)
            {
                return null;
            }

            if (source.IndexOf("password", StringComparison.OrdinalIgnoreCase) > 0)
            {
               return  _passwordClean.Replace(source, "***");
            }

            return source;
        }
    }
}