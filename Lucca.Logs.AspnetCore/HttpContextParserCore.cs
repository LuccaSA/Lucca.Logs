using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lucca.Logs.Shared;
using Microsoft.AspNetCore.Http;
using StackExchange.Exceptional;

namespace Lucca.Logs.AspnetCore
{
    public sealed class HttpContextParserCore : HttpContextParserBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextParserCore(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpRequest Request => _httpContextAccessor?.HttpContext?.Request;

        public override string ExtractUrl(Uripart uriPart)
        {
            if (Request == null)
                return null;

            var urlBuilder = new StringBuilder();
            if ((uriPart & Uripart.Scheme) == Uripart.Scheme && !String.IsNullOrWhiteSpace(Request.Scheme))
            {
                urlBuilder.Append(Request.Scheme + "://");
            }
            if ((uriPart & Uripart.Host) == Uripart.Host)
            {
                urlBuilder.Append(Request.Host.Host);
            }
            if ((uriPart & Uripart.Port) == Uripart.Port && Request.Host.Port > 0)
            {
                urlBuilder.Append(":" + Request.Host.Port);
            }
            if ((uriPart & Uripart.Path) == Uripart.Path)
            {
                urlBuilder.Append(Request.PathBase.ToUriComponent());
                urlBuilder.Append(Request.Path.ToUriComponent());
            }
            if ((uriPart & Uripart.Query) == Uripart.Query)
            {
                urlBuilder.Append(Request.QueryString.Value.ClearQueryStringPassword());
            }
            return urlBuilder.ToString();
        }

        public override string GetMethod() => Request?.Method;

        public override bool ContainsHeader(string header)
        {
            if (Request == null)
                return false;

            return Request.Headers.ContainsKey(header);
        }

        public override string GetHeader(string header) => Request?.Headers[header];

        public override string Ip => Request?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public override string TryGetPayload()
        {
            if (Request == null || !Request.Body.CanRead || !Request.Body.CanSeek || Request.Body.Length == 0)
            {
                return null;
            }
            string documentContents = null;
            try
            {
                using (var stream = new MemoryStream())
                {
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    Request.Body.CopyTo(stream);
                    documentContents = Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception)
            {
                // discard exception
            }
            return documentContents;
        }

        public override Guid? ExceptionalLog(Exception exception, Dictionary<string, string> customData, string categoryName, string appName)
        {
            if (exception == null)
            {
                return null;
            }

            Error error;

            if (Request?.HttpContext != null)
            {
                error = exception.Log(Request.HttpContext, categoryName, false, customData, appName);
            }
            else
            {
                error = exception.LogNoContext(categoryName, false, customData, appName);
            }

            return error?.GUID;
        }

        public override bool CanRead()
        {
            return _httpContextAccessor.HttpContext != null;
        }
    }
}
