using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using Lucca.Logs.Shared;
using StackExchange.Exceptional;
using System.Net;

namespace Lucca.Logs.AspnetLegacy
{
    public sealed class HttpContextParserLegacy : IHttpContextParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextParserLegacy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
       
        public Guid? ExceptionalLog(Exception? exception, Dictionary<string, string?> customData, string categoryName, string appName)
        {
            if (exception is null)
            {
                return null;
            }

            Error GetError()
            {
                var ctx = _httpContextAccessor?.HttpContext;
                if (ctx is not null)
                {
                    return exception.Log(ctx, categoryName, false, customData, appName);
                }
                return exception.LogNoContext(categoryName, false, customData, appName);
            }

            return GetError()?.GUID;
        }

        public IHttpContextRequest? HttpRequestAccessor()
        {
            var request = _httpContextAccessor?.HttpContext?.Request;
            return request is not null ? new HttpContextRequestLegacy(request) : null;
        }
    }

    internal class HttpContextRequestLegacy : IHttpContextRequest
    {
        private const string SchemeEnd = "://";
        private const string Colon = ":";

        public HttpRequest HttpRequest { get; }

        public HttpContextRequestLegacy(HttpRequest httpRequest)
        {
            HttpRequest = httpRequest;
        }

        public string ExtractUrl(Uriparts uriPart)
        {
            var urlBuilder = new StringBuilder();
            if ((uriPart & Uriparts.Scheme) == Uriparts.Scheme && !string.IsNullOrWhiteSpace(HttpRequest.Url.Scheme))
            {
                urlBuilder.Append(HttpRequest.Url.Scheme + SchemeEnd);
            }
            if ((uriPart & Uriparts.Host) == Uriparts.Host)
            {
                urlBuilder.Append(HttpRequest.Url.DnsSafeHost);
            }
            if ((uriPart & Uriparts.Port) == Uriparts.Port && HttpRequest.Url.Port > 0)
            {
                urlBuilder.Append(Colon + HttpRequest.Url.Port);
            }
            if ((uriPart & Uriparts.Path) == Uriparts.Path)
            {
                urlBuilder.Append(HttpRequest.Url.LocalPath);
            }
            if ((uriPart & Uriparts.Query) == Uriparts.Query)
            {
                urlBuilder.Append(HttpRequest.Url.Query.ClearQueryStringPassword());
            }
            return urlBuilder.ToString();
        }

        public string? GetHeader(string header) => HttpRequest.Headers.Get(header);

        public string? TryGetBodyContent()
        {
            if (HttpRequest.InputStream.Length == 0)
            {
                return null;
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    HttpRequest.InputStream.Seek(0, SeekOrigin.Begin);
                    HttpRequest.InputStream.CopyTo(stream);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception)
            {
                // discard exception
            }
            return null;
        }

        public string GetMethod() => HttpRequest.HttpMethod;

        public IPAddress? HostAddress()
        {
            var cfIp = HttpRequest.Headers.Get(LogMeta._cfConnectingIPHeader);
            if (cfIp != null && IPAddress.TryParse(cfIp, out var ip))
            {
                return ip;
            }

            var xfwdip = HttpRequest.Headers.Get(LogMeta._forwardedHeader);
            if (xfwdip != null && IPAddress.TryParse(xfwdip, out ip))
            {
                return ip;
            }

            return IPAddress.TryParse(HttpRequest.UserHostAddress ?? string.Empty, out ip) ? ip : null;
        }
    }
}
