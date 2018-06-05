using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using NLog.Layouts;

namespace Lucca.Logs
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

        internal static Dictionary<string, string> GatherData(Exception e, HttpRequest httpRequest, bool isError, string appName)
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

        internal static Dictionary<string, string> GatherData(HttpRequest httpRequest, bool isError, string appName)
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
            if (httpRequest.Headers.ContainsKey(_luccaForwardedHeader))
            {
                ip = httpRequest.Headers[_forwardedHeader];
            }
            
            if (String.IsNullOrEmpty(ip))
            {
                ip = httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            data.Add(_hostAddress, ip);

            data.Add(_userAgent, httpRequest.Headers["User-Agent"].ToString());

            if (!isError || !httpRequest.Body.CanRead || !httpRequest.Body.CanSeek)
            {
                return data;
            }

            string documentContents = null;
            try
            {
                using (var stream = new MemoryStream())
                {
                    httpRequest.Body.Seek(0, SeekOrigin.Begin);
                    httpRequest.Body.CopyTo(stream);
                    documentContents = Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception)
            {
            }

            if (!String.IsNullOrEmpty(documentContents))
            {
                data.Add(_rawPostedData, documentContents);
            }

            return data;
        }

        public static string ExtractUrl(this HttpRequest httpRequest, UriPart uriPart)
        {
            var urlBuilder = new StringBuilder();
            if ((uriPart & UriPart.Scheme) == UriPart.Scheme && !String.IsNullOrWhiteSpace(httpRequest.Scheme))
            {
                urlBuilder.Append(httpRequest.Scheme + "://");
            }
            if ((uriPart & UriPart.Host) == UriPart.Host)
            {
                urlBuilder.Append(httpRequest.Host.Host);
            }
            if ((uriPart & UriPart.Port) == UriPart.Port && httpRequest.Host.Port > 0)
            {
                urlBuilder.Append(":" + httpRequest.Host.Port);
            }
            if ((uriPart & UriPart.Path) == UriPart.Path)
            {
                urlBuilder.Append(httpRequest.PathBase.ToUriComponent());
                urlBuilder.Append(httpRequest.Path.ToUriComponent());
            }
            if ((uriPart & UriPart.Query) == UriPart.Query)
            {
                urlBuilder.Append(httpRequest.QueryString.Value);
            }
            return urlBuilder.ToString();
        }
        
        public static class LogExtractor
        {
            public static Func<Exception, IEnumerable<KeyValuePair<string, string>>> CustomKeys { get; set; }
        }

        [Flags]
        internal enum UriPart
        {
            None = 0,
            Scheme = 1,
            Host = 1 << 1,
            Port = 1 << 2,
            Path = 1 << 3,
            Query = 1 << 4,
            Full = Scheme | Host | Port | Path | Query
        }
    }
}