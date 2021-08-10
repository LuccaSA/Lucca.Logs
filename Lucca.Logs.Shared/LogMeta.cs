namespace Lucca.Logs.Shared
{
    internal static class LogMeta
    {
        internal const string Link = "OpserverLink";
        internal const string _exceptionDetails = "ExceptionDetails";

        internal const string _warning = "WARNING";
        internal const string _pageRest = "Page REST";
        internal const string _pageRest2 = "PageREST";
        internal const string _page = "Page";
        internal const string _verb = "method";
        internal const string _hostAddress = "ipAddress";
        internal const string _userAgent = "userAgent";
        public const string RawPostedData = "RawPostedData";

        internal const string _appName = "AppName";
        internal const string _serverName = "servername";
        internal const string _appPool = "AppPool";
        internal const string _uri = "Uri";

        internal const string _principal = "Principal";
        internal const string _exceptionMethodName = "ExceptionMethodName";
        internal const string _exceptionClassName = "ExceptionClassName";
        internal const string _exceptionNamespace = "ExceptionNamespace";
        internal const string _httpLikeExceptionStatus = "HttpLikeExceptionStatus";

        internal const string _luccaForwardedHeader = "X-Forwarded-By-Lucca";
        internal const string _forwardedHeader = "X-Forwarded-For";
        internal const string _correlationId = "X-Correlation-ID";

        internal const string _traceId = "dd.trace_id";
        internal const string _spanId = "dd.span_id";

        private static string[] Keys => new[]
        {
            _warning,
            _pageRest,
            _page,
            _verb,
            _hostAddress,
            _userAgent,
            RawPostedData,
            _principal,
            _appName,
            _serverName,
            _appPool,
            _exceptionMethodName,
            _exceptionClassName,
            _exceptionNamespace,
            _httpLikeExceptionStatus,
            _uri,
            _correlationId,
            _traceId,
            _spanId,
        };
    }
}