namespace Lucca.Logs.Shared
{
    internal static class LogMeta
    {
        internal const string Link = "OpserverLink";
        internal const string ExceptionDetails = "ExceptionDetails";

        internal const string Warning = "WARNING";
        internal const string PageRest = "Page REST";
        internal const string PageRest2 = "PageREST";
        internal const string Page = "Page";
        internal const string Verb = "method";
        internal const string HostAddress = "ipAddress";
        internal const string UserAgent = "userAgent";
        public const string RawPostedData = "RawPostedData";

        internal const string AppName = "AppName";
        internal const string ServerName = "servername";
        internal const string AppPool = "AppPool";
        internal const string Uri = "Uri";

        internal const string Principal = "Principal";
        internal const string ExceptionMethodName = "ExceptionMethodName";
        internal const string ExceptionClassName = "ExceptionClassName";
        internal const string ExceptionNamespace = "ExceptionNamespace";
        internal const string HttpLikeExceptionStatus = "HttpLikeExceptionStatus";

        internal const string LuccaForwardedHeader = "X-Forwarded-By-Lucca";
        internal const string ForwardedHeader = "X-Forwarded-For";
        internal const string CorrelationId = "X-Correlation-ID";

        internal const string TraceId = "dd.trace_id";
        internal const string SpanId = "dd.span_id";

        // exceptional
        internal const string Guid = "Guid";
        internal const string ExceptionHash = "Hash";
        internal const string Exception = "Ex";
        internal const string ExceptionType = "ExType";
        internal const string ExceptionMessage = "ExMessage";
        internal const string ExceptionSource = "ExSource";

        internal const string StackTrace = "StackTrace";
        internal const string LogSiteStackTrace = "LogSite";
    }
}