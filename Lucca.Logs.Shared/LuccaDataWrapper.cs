using System;
using System.Collections.Generic;
using NLog.Layouts;

namespace Lucca.Logs.Shared
{
    internal static class LuccaDataWrapper
    {
        internal const string Link = "OpserverLink";
        private const string _exceptionDetails = "ExceptionDetails";

        private const string _warning = "WARNING";
        private const string _pageRest = "Page REST";
        private const string _page = "Page";
        private const string _verb = "method";
        private const string _hostAddress = "ipAddress";
        private const string _userAgent = "userAgent";
        private const string _rawPostedData = "RawPostedData";

        private const string _appName = "AppName";
        private const string _serverName = "servername";
        private const string _appPool = "AppPool";
        private const string _uri = "Uri";

        private const string _principal = "Principal";
        private const string _exceptionMethodName = "ExceptionMethodName";
        private const string _exceptionClassName = "ExceptionClassName";
        private const string _exceptionNamespace = "ExceptionNamespace";
        private const string _httpLikeExceptionStatus = "HttpLikeExceptionStatus";

        private const string _luccaForwardedHeader = "X-Forwarded-By-Lucca";
        private const string _forwardedHeader = "X-Forwarded-For";

        internal static string[] Keys() => new[]
        {
            _warning,
            _pageRest,
            _page,
            _verb,
            _hostAddress,
            _userAgent,
            _rawPostedData,
            _principal,
            _appName,
            _serverName,
            _appPool,
            _exceptionMethodName,
            _exceptionClassName,
            _exceptionNamespace,
            _httpLikeExceptionStatus,
            _uri
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

            foreach (string key in Keys())
            {
                jsonLayout.Attributes.Add(new JsonAttribute(key, "${event-properties:item=" + key + "}"));
            }
            jsonLayout.Attributes.Add(new JsonAttribute(_exceptionDetails, "${event-properties:item=" + _exceptionDetails + "}") { Encode = false });
            return jsonLayout;
        }

        internal static Dictionary<string, string> GatherData(Exception e, IHttpContextWrapper httpRequest, bool isError, string appName)
        {
            Dictionary<string, string> data = GatherData(httpRequest, isError, appName);
            if (LogExtractor.CustomKeys != null)
            {
                foreach (KeyValuePair<string, string> kv in LogExtractor.CustomKeys(e))
                {
                    data.Add(kv.Key, kv.Value);
                }
            }
            return data;
        }

        internal static Dictionary<string, string> GatherData(IHttpContextWrapper httpRequest, bool isError, string appName)
        {
            var data = new Dictionary<string, string>();
            if (!String.IsNullOrEmpty(appName))
            {
                data.Add(_appName, appName);
            }

            if (httpRequest == null)
            {
                data.Add(_warning, "HttpContext.Current is null");
                return data;
            }

            data.Add(_pageRest, httpRequest.ExtractUrl(UriPart.Path | UriPart.Query));
            data.Add(_page, httpRequest.ExtractUrl(UriPart.Full));
            data.Add(_verb, httpRequest.Method);
            data.Add(_uri, httpRequest.ExtractUrl(UriPart.Path));
            data.Add(_serverName, httpRequest.ExtractUrl(UriPart.Host));
            // https://stackoverflow.com/a/39139875
            data.Add(_appPool, Environment.GetEnvironmentVariable("APP_POOL_ID", EnvironmentVariableTarget.Process));

            // Récupération de l'IP forwardée par HAProxy, et fallback UserHostAddress
            string ip = null;
            if (httpRequest.ContainsHeader(_luccaForwardedHeader))
            {
                ip = httpRequest.GetHEader(_forwardedHeader);
            }

            if (String.IsNullOrEmpty(ip))
            {
                ip = httpRequest.Ip;
            }

            data.Add(_hostAddress, ip);

            data.Add(_userAgent, httpRequest.GetHEader("User-Agent"));

            if (!isError)
            {
                return data;
            }

            string documentContents = httpRequest.TryGetBodyContent();
             
            if (!String.IsNullOrEmpty(documentContents))
            {
                data.Add(_rawPostedData, documentContents);
            }

            return data;
        }

        public static class LogExtractor
        {
            public static Func<Exception, IEnumerable<KeyValuePair<string, string>>> CustomKeys { get; set; }
        }

    }
}