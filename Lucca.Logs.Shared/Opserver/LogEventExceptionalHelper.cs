using Lucca.Logs.Shared.Exceptional;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace Lucca.Logs.Shared.Opserver
{
    public static class LogEventExceptionalHelper
    {
        public static Error ToExceptionalError(this LogEvent logEvent)
        {
            if (logEvent.Exception == null)
                return null;

            var error = new Error();
            error.Exception = logEvent.Exception;
            error.GUID = Guid.NewGuid();
            logEvent.SetGuid(error.GUID);

            error.ApplicationName = logEvent.GetScalar(LogMeta.AppName);
            error.Category = logEvent.GetScalar(Constants.SourceContextPropertyName);
            error.MachineName = Environment.MachineName;
            error.Type = logEvent.GetScalar(LogMeta.ExceptionType);
            error.Message = logEvent.GetScalar(LogMeta.ExceptionMessage);
            error.Source = logEvent.GetScalar(LogMeta.ExceptionSource);
            error.Detail = logEvent.GetScalar(LogMeta.Exception);
            error.CreationDate = DateTime.UtcNow;
            error.DuplicateCount = 1;
            error.CustomData = new Dictionary<string, string>()
            {
                {LogMeta.Warning, logEvent.GetScalar(LogMeta.Warning)},
                {LogMeta.PageRest, logEvent.GetScalar(LogMeta.PageRest)},
                {LogMeta.PageRest2, logEvent.GetScalar(LogMeta.PageRest2)},
                {LogMeta.Page, logEvent.GetScalar(LogMeta.Page)},
                {LogMeta.Verb, logEvent.GetScalar(LogMeta.Verb)},
                {LogMeta.Uri, logEvent.GetScalar(LogMeta.Uri)},
                {LogMeta.ServerName, logEvent.GetScalar(LogMeta.ServerName)},
                {LogMeta.CorrelationId, logEvent.GetScalar(LogMeta.CorrelationId)},
                {LogMeta.HostAddress, logEvent.GetScalar(LogMeta.HostAddress)},
                {LogMeta.UserAgent, logEvent.GetScalar(LogMeta.UserAgent)},
                {LogMeta.RawPostedData, logEvent.GetScalar(LogMeta.RawPostedData)}
            };

            error.Detail += logEvent.GetScalar(LogMeta.LogSiteStackTrace);
            error.ErrorHash = logEvent.GetScalar(LogMeta.ExceptionHash).AsInt();

            return error;
        }

        private static void SetGuid(this LogEvent logEvent, Guid guid)
        {
            logEvent.AddPropertyIfAbsent(new LogEventProperty(LogMeta.Guid, new ScalarValue(guid.ToString("N"))));
        }

        private static int? AsInt(this string source) => int.TryParse(source, out var value) ? (int?)value : null;

        private static string GetScalar(this LogEvent logEvent, string key)
        {
            if (logEvent.Properties.TryGetValue(key, out var propertyValue))
            {
                if (propertyValue is ScalarValue sv && sv.Value is string value)
                {
                    return value;
                }
            }
            return null;
        }

        public static string Truncate(this string s, int maxLength) =>
            (s.HasValue() && s.Length > maxLength) ? s.Remove(maxLength) : s;

        private static bool HasValue(this string s) => !string.IsNullOrEmpty(s);
    }
}