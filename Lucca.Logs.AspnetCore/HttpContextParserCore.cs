using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lucca.Logs.Shared;
using Microsoft.AspNetCore.Http;
using StackExchange.Exceptional;
using System.Net;

namespace Lucca.Logs.AspnetCore
{
    public sealed class HttpContextParserCore : IHttpContextParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextParserCore(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        public Guid? ExceptionalLog(Exception exception, Dictionary<string, string> customData, string categoryName, string appName)
        {
            if (exception == null)
            {
                return null;
            }

            Error error;

            var request = _httpContextAccessor?.HttpContext?.Request;

            if (request?.HttpContext != null)
            {
                error = exception.Log(request.HttpContext, categoryName, false, customData, appName);
            }
            else
            {
                error = exception.LogNoContext(categoryName, false, customData, appName);
            }

            return error?.GUID;
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
            var request = (httpRequest as HttpContextRequestCore)?.HttpRequest;
            return request?.Method;
        }

        public IPAddress HostAddress(IHttpContextRequest httpRequest)
        {
            var request = (httpRequest as HttpContextRequestCore)?.HttpRequest;
            if (request == null)
                return null;
            if (request.Headers.ContainsKey(LogMeta._cfConnectingIPHeader)
                && IPAddress.TryParse(request.Headers[LogMeta._cfConnectingIPHeader], out var ip))
            {
                return ip;
            }

            if (request.Headers.ContainsKey(LogMeta._luccaForwardedHeader)
                && IPAddress.TryParse(request.Headers[LogMeta._forwardedHeader], out ip))
            {
                return ip;
            }

            return request.HttpContext.Connection.RemoteIpAddress;
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
