using System;
using System.Collections.Generic;
using NLog.Layouts;

namespace Lucca.Logs.Shared
{
    internal static class LuccaDataWrapper
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
            _correlationId
        };

        internal static JsonLayout GenerateJsonLayout()
        {
            var jsonLayout = new JsonLayout();
            jsonLayout.Attributes.Add(new JsonAttribute("date", "${longdate}"));
            jsonLayout.Attributes.Add(new JsonAttribute("level", "${level:upperCase=true}"));
            jsonLayout.Attributes.Add(new JsonAttribute("message", "${message}"));
            jsonLayout.Attributes.Add(new JsonAttribute("hostname", "${machinename}"));
            jsonLayout.Attributes.Add(new JsonAttribute("exception", "${exception:format=ShortType,Message,Method,Data}"));
            jsonLayout.Attributes.Add(new JsonAttribute(Link, "${event-properties:item=" + Link + "}"));

            foreach (string key in Keys)
            {
                jsonLayout.Attributes.Add(new JsonAttribute(key, "${event-properties:item=" + key + "}"));
            }
            jsonLayout.Attributes.Add(new JsonAttribute(_exceptionDetails, "${event-properties:item=" + _exceptionDetails + "}") { Encode = false });
            return jsonLayout;
        }

        internal static Dictionary<string, string> GatherData(IContextParser contextParser, bool isError, string appName)
        {
            var data = new Dictionary<string, string>(16);
            if (!string.IsNullOrEmpty(appName))
            {
                data.Add(_appName, appName);
            }

            if (contextParser == null)
            {
                data.Add(_warning, "HttpContext.Current is null");
                return data;
            }

            data.Add(_pageRest, contextParser.GetPageRest());
            data.Add(_pageRest2, contextParser.GetPageRest2());
            data.Add(_page, contextParser.GetPage());
            data.Add(_verb, contextParser.GetMethod());
            data.Add(_uri, contextParser.GetUri());
            data.Add(_serverName, contextParser.GetServerName());
            // https://stackoverflow.com/a/39139875
            data.Add(_appPool, Environment.GetEnvironmentVariable("APP_POOL_ID", EnvironmentVariableTarget.Process));

            data.Add(_correlationId, contextParser.GetCorrelationId());
            data.Add(_hostAddress, contextParser.GetClientIp());

            data.Add(_userAgent, contextParser.GetUserAgent());

            if (!isError)
            {
                return data;
            }

            string documentContents = contextParser.TryGetPayload();

            if (!string.IsNullOrEmpty(documentContents))
            {
                data.Add(RawPostedData, documentContents);
            }

            return data;
        }
    }
}
