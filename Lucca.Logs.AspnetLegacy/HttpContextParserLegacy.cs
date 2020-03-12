using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using Lucca.Logs.Abstractions;
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
        public bool IsContextDefined => _httpContextAccessor?.HttpContext != null;

        private HttpRequest Request
        {
            get
            {
                HttpRequest request = null;
                try
                {
                    request = _httpContextAccessor?.HttpContext?.Request;
                }
                catch (Exception)
                {
                   // ignored
                }
                return request;
            }
        }

        public string ExtractUrl(UriParts uriPart)
        {
            if (Request == null)
                return null;

            var urlBuilder = new StringBuilder();
            if ((uriPart & UriParts.Scheme) == UriParts.Scheme && !string.IsNullOrWhiteSpace(Request.Url.Scheme))
            {
                urlBuilder.Append(Request.Url.Scheme + "://");
            }
            if ((uriPart & UriParts.Host) == UriParts.Host)
            {
                urlBuilder.Append(Request.Url.DnsSafeHost);
            }
            if ((uriPart & UriParts.Port) == UriParts.Port && Request.Url.Port > 0)
            {
                urlBuilder.Append(":" + Request.Url.Port);
            }
            if ((uriPart & UriParts.Path) == UriParts.Path)
            {
                urlBuilder.Append(Request.Url.LocalPath);
            }
            if ((uriPart & UriParts.Query) == UriParts.Query)
            {
                urlBuilder.Append(Request.Url.Query.ClearQueryStringPassword());
            }
            return urlBuilder.ToString();
        }

        public string Method => Request?.HttpMethod;

        public bool ContainsHeader(string header)
        {
            return Request?.Headers.Get(header) != null;
        }

        public string GetHeader(string header) => Request?.Headers.Get(header);

        public string Ip => Request?.UserHostAddress;

        public string TryGetBodyContent()
        {
            string documentContents = null;
            try
            {
                if (Request == null || Request.InputStream.Length == 0)
                {
                    return null;
                } 
                using (var stream = new MemoryStream())
                {
                    Request.InputStream.Seek(0, SeekOrigin.Begin);
                    Request.InputStream.CopyTo(stream);
                    documentContents = Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception)
            {
                // discard exception
            }
            return documentContents;
        }

        public Guid? ExceptionalLog(Exception exception, Dictionary<string, string> customData, string categoryName, string appName)
        {
            if (exception == null)
            {
                return null;
            }

            Error error;
            var ctx = _httpContextAccessor?.HttpContext;
            if (ctx != null)
            {
                error = exception.Log(ctx, categoryName, false, customData, appName);
            }
            else
            {
                error = exception.LogNoContext(categoryName, false, customData, appName);
            }

            return error?.GUID;
        }
    }
}
