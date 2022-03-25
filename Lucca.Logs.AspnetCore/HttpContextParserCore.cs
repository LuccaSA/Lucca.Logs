using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lucca.Logs.Shared;
using Microsoft.AspNetCore.Http;
using StackExchange.Exceptional;

namespace Lucca.Logs.AspnetCore
{
    public sealed class HttpContextParserCore : IHttpContextParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextParserCore(IHttpContextAccessor httpContextAccessor)
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
            var req = _httpContextAccessor?.HttpContext?.Request;
            return req is not null ? new HttpContextRequestCore(req) : null;
        }
    }

    public class HttpContextRequestCore : IHttpContextRequest
    {
        private const string SchemeEnd = "://";
        private const string Colon = ":";

        public HttpRequest HttpRequest { get; }

        public HttpContextRequestCore(HttpRequest httpRequest)
        {
            HttpRequest = httpRequest;
        }

        public string ExtractUrl(Uriparts uriPart)
        {
            var urlBuilder = new StringBuilder();
            if ((uriPart & Uriparts.Scheme) == Uriparts.Scheme && !string.IsNullOrWhiteSpace(HttpRequest.Scheme))
            {
                urlBuilder.Append(HttpRequest.Scheme + SchemeEnd);
            }
            if ((uriPart & Uriparts.Host) == Uriparts.Host)
            {
                urlBuilder.Append(HttpRequest.Host.Host);
            }
            if ((uriPart & Uriparts.Port) == Uriparts.Port && HttpRequest.Host.Port > 0)
            {
                urlBuilder.Append(Colon + HttpRequest.Host.Port);
            }
            if ((uriPart & Uriparts.Path) == Uriparts.Path)
            {
                urlBuilder.Append(HttpRequest.PathBase.ToUriComponent());
                urlBuilder.Append(HttpRequest.Path.ToUriComponent());
            }
            if ((uriPart & Uriparts.Query) == Uriparts.Query)
            {
                urlBuilder.Append(HttpRequest.QueryString.Value.ClearQueryStringPassword());
            }
            return urlBuilder.ToString();
        }

        public string? GetHeader(string header) => HttpRequest.Headers[header];

        public string? TryGetBodyContent()
        {
            try
            {
                if (!HttpRequest.Body.CanRead || !HttpRequest.Body.CanSeek || HttpRequest.Body.Length == 0)
                {
                    return null;
                }

                using (var stream = new MemoryStream())
                {
                    HttpRequest.Body.Seek(0, SeekOrigin.Begin);
                    HttpRequest.Body.CopyTo(stream);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception)
            {
                // discard exception
                return null;
            }
        }

        public string GetMethod() => HttpRequest.Method;

        public string? HostAddress()
        {
            string? ip = null;
            if (HttpRequest.Headers.ContainsKey(LogMeta._luccaForwardedHeader))
            {
                ip = HttpRequest.Headers[LogMeta._forwardedHeader];
            }
            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpRequest.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            }
            return ip;
        }
    }
}
