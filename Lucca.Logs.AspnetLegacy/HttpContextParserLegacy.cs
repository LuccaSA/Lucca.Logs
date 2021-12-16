using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using Lucca.Logs.Shared;
using StackExchange.Exceptional;

namespace Lucca.Logs.AspnetLegacy
{
    public sealed class HttpContextParserLegacy : IHttpContextParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextParserLegacy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
       
        public Guid? ExceptionalLog(Exception exception, Dictionary<string, string?> customData, string categoryName, string appName)
        {
            if (exception is null)
            {
                return null;
            }

            Error error;
            var ctx = _httpContextAccessor?.HttpContext;
            if (ctx is not null)
            {
                error = exception.Log(ctx, categoryName, false, customData, appName);
            }
            else
            {
                error = exception.LogNoContext(categoryName, false, customData, appName);
            }

            return error?.GUID;
        }

        public string? ExtractUrl(Uripart uriPart, IHttpContextRequest httpRequest)
        {
            var request = (httpRequest as HttpContextRequestLegacy).HttpRequest;
            if (request is null)
            {
                return null;
            }

            var urlBuilder = new StringBuilder();
            if ((uriPart & Uripart.Scheme) == Uripart.Scheme && !string.IsNullOrWhiteSpace(request.Url.Scheme))
            {
                urlBuilder.Append(request.Url.Scheme + "://");
            }
            if ((uriPart & Uripart.Host) == Uripart.Host)
            {
                urlBuilder.Append(request.Url.DnsSafeHost);
            }
            if ((uriPart & Uripart.Port) == Uripart.Port && request.Url.Port > 0)
            {
                urlBuilder.Append(":" + request.Url.Port);
            }
            if ((uriPart & Uripart.Path) == Uripart.Path)
            {
                urlBuilder.Append(request.Url.LocalPath);
            }
            if ((uriPart & Uripart.Query) == Uripart.Query)
            {
                urlBuilder.Append(request.Url.Query.ClearQueryStringPassword());
            }
            return urlBuilder.ToString();
        }

        public bool ContainsHeader(string header, IHttpContextRequest httpRequest)
        {
            var request = (httpRequest as HttpContextRequestLegacy).HttpRequest;
            return request?.Headers.Get(header) is not null;
        }

        public string? GetHeader(string header, IHttpContextRequest httpRequest)
        {
            var request = (httpRequest as HttpContextRequestLegacy).HttpRequest;
            return request?.Headers.Get(header);
        }

        public string? TryGetBodyContent(IHttpContextRequest httpRequest)
        {
            var request = (httpRequest as HttpContextRequestLegacy).HttpRequest;
            string? documentContents = null;
            try
            {
                if (request is null || request.InputStream.Length == 0)
                {
                    return null;
                }
                using (var stream = new MemoryStream())
                {
                    request.InputStream.Seek(0, SeekOrigin.Begin);
                    request.InputStream.CopyTo(stream);
                    documentContents = Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception)
            {
                // discard exception
            }
            return documentContents;
        }

        public IHttpContextRequest? HttpRequestAccessor()
        {
            HttpRequest? request = null;
            try
            {
                request = _httpContextAccessor?.HttpContext?.Request;
            }
            catch (Exception)
            {
                // ignored
            } 
            return request is not null ? new HttpContextRequestLegacy(request) : null;
        }

        public string? GetMethod(IHttpContextRequest httpRequest)
        {
            var request = (httpRequest as HttpContextRequestLegacy).HttpRequest;
            return request?.HttpMethod;
        }

        public string? HostAddress(IHttpContextRequest httpRequest)
        {
            var request = (httpRequest as HttpContextRequestLegacy).HttpRequest;
            string? ip = null;
            if (request.Headers.Get(LogMeta._luccaForwardedHeader) is not null)
            {
                ip = request.Headers[LogMeta._forwardedHeader];
            }
            if (string.IsNullOrEmpty(ip))
            {
                ip = request?.UserHostAddress;
            }
            return ip;
        }
    }

    public class HttpContextRequestLegacy : IHttpContextRequest
    {
        public HttpRequest HttpRequest { get; }

        public HttpContextRequestLegacy(HttpRequest httpRequest)
        {
            HttpRequest = httpRequest;
        }
    }
}
