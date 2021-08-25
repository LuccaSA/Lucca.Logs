using System;
using System.IO;
using System.Text;
using Lucca.Logs.Shared;
using Microsoft.AspNetCore.Http;

namespace Lucca.Logs.AspnetCore
{
    public sealed class HttpContextParserCore : IHttpContextParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextParserCore(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IHttpContextRequest HttpRequestAccessor()
        {
            var req = _httpContextAccessor?.HttpContext?.Request;
            return req != null ? new HttpContextRequestCore(req) : null;
        }

        public string ExtractUrl(Uripart uriPart, IHttpContextRequest httpRequest)
        {
            var request = (httpRequest as HttpContextRequestCore).HttpRequest;
            if (request == null)
                return null;

            var urlBuilder = new StringBuilder();
            if ((uriPart & Uripart.Scheme) == Uripart.Scheme && !String.IsNullOrWhiteSpace(request.Scheme))
            {
                urlBuilder.Append(request.Scheme + "://");
            }
            if ((uriPart & Uripart.Host) == Uripart.Host)
            {
                urlBuilder.Append(request.Host.Host);
            }
            if ((uriPart & Uripart.Port) == Uripart.Port && request.Host.Port > 0)
            {
                urlBuilder.Append(":" + request.Host.Port);
            }
            if ((uriPart & Uripart.Path) == Uripart.Path)
            {
                urlBuilder.Append(request.PathBase.ToUriComponent());
                urlBuilder.Append(request.Path.ToUriComponent());
            }
            if ((uriPart & Uripart.Query) == Uripart.Query)
            {
                urlBuilder.Append(request.QueryString.Value.ClearQueryStringPassword());
            }
            return urlBuilder.ToString();
        }

        public bool ContainsHeader(string header, IHttpContextRequest httpRequest)
        {
            var request = (httpRequest as HttpContextRequestCore).HttpRequest;
            if (request == null)
                return false;

            return request.Headers.ContainsKey(header);
        }

        public string GetHeader(string header, IHttpContextRequest httpRequest)
        {
            var request = (httpRequest as HttpContextRequestCore).HttpRequest;
            return request?.Headers[header];
        }

        public string TryGetBodyContent(IHttpContextRequest httpRequest)
        {
            var request = (httpRequest as HttpContextRequestCore).HttpRequest;
            try
            {
                if (request == null || !request.Body.CanRead || !request.Body.CanSeek || request.Body.Length == 0)
                {
                    return null;
                }

                using (var stream = new MemoryStream())
                {
                    request.Body.Seek(0, SeekOrigin.Begin);
                    request.Body.CopyTo(stream);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception)
            {
                // discard exception
                return null;
            }
        }

        public string GetMethod(IHttpContextRequest httpRequest)
        {
            var request = (httpRequest as HttpContextRequestCore).HttpRequest;
            return request?.Method;
        }

        public string HostAddress(IHttpContextRequest httpRequest)
        {
            var request = (httpRequest as HttpContextRequestCore).HttpRequest;
            string ip = null;
            if (request.Headers.ContainsKey(LogMeta.LuccaForwardedHeader))
            {
                ip = request.Headers[LogMeta.ForwardedHeader];
            }
            if (string.IsNullOrEmpty(ip))
            {
                ip = request?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            }
            return ip;
        }
    }

    public class HttpContextRequestCore : IHttpContextRequest
    {
        public HttpRequest HttpRequest { get; }

        public HttpContextRequestCore(HttpRequest httpRequest)
        {
            HttpRequest = httpRequest;
        }
    }
}
