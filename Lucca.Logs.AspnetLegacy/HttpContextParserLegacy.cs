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
    }

    public class HttpContextRequestLegacy : IHttpContextRequest
    {
        public HttpRequest HttpRequest { get; }

        public HttpContextRequestLegacy(HttpRequest httpRequest)
        {
            HttpRequest = httpRequest;
        }

        public string ExtractUrl(Uripart uriPart)
        {
            var urlBuilder = new StringBuilder();
            if ((uriPart & Uripart.Scheme) == Uripart.Scheme && !string.IsNullOrWhiteSpace(HttpRequest.Url.Scheme))
            {
                urlBuilder.Append(HttpRequest.Url.Scheme + "://");
            }
            if ((uriPart & Uripart.Host) == Uripart.Host)
            {
                urlBuilder.Append(HttpRequest.Url.DnsSafeHost);
            }
            if ((uriPart & Uripart.Port) == Uripart.Port && HttpRequest.Url.Port > 0)
            {
                urlBuilder.Append(":" + HttpRequest.Url.Port);
            }
            if ((uriPart & Uripart.Path) == Uripart.Path)
            {
                urlBuilder.Append(HttpRequest.Url.LocalPath);
            }
            if ((uriPart & Uripart.Query) == Uripart.Query)
            {
                urlBuilder.Append(HttpRequest.Url.Query.ClearQueryStringPassword());
            }
            return urlBuilder.ToString();
        }

        public bool ContainsHeader(string header)
        {
            return HttpRequest.Headers.Get(header) is not null;
        }

        public string? GetHeader(string header)
        {
            return HttpRequest.Headers.Get(header);
        }

        public string? TryGetBodyContent()
        {
            if (HttpRequest.InputStream.Length == 0)
            {
                return null;
            }

            string? documentContents = null;
            try
            {
                using (var stream = new MemoryStream())
                {
                    HttpRequest.InputStream.Seek(0, SeekOrigin.Begin);
                    HttpRequest.InputStream.CopyTo(stream);
                    documentContents = Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception)
            {
                // discard exception
            }
            return documentContents;
        }

        public string GetMethod()
        {
            return HttpRequest.HttpMethod;
        }

        public string? HostAddress()
        {
            string? ip = null;
            if (HttpRequest.Headers.Get(LogMeta._luccaForwardedHeader) is not null)
            {
                ip = HttpRequest.Headers[LogMeta._forwardedHeader];
            }
            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpRequest.UserHostAddress;
            }
            return ip;
        }
    }
}
