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

        private HttpRequest Request
        {
            get
            {
                HttpRequest request = null;
                try
                {
                    request = _httpContextAccessor?.HttpContext?.Request;
                }
                catch (Exception e)
                {
                   // ignored
                }
                return request;
            }
        }

        public string ExtractUrl(UriPart uriPart)
        {
            if (Request == null)
                return null;

            var urlBuilder = new StringBuilder();
            if ((uriPart & UriPart.Scheme) == UriPart.Scheme && !String.IsNullOrWhiteSpace(Request.Url.Scheme))
            {
                urlBuilder.Append(Request.Url.Scheme + "://");
            }
            if ((uriPart & UriPart.Host) == UriPart.Host)
            {
                urlBuilder.Append(Request.Url.DnsSafeHost);
            }
            if ((uriPart & UriPart.Port) == UriPart.Port && Request.Url.Port > 0)
            {
                urlBuilder.Append(":" + Request.Url.Port);
            }
            if ((uriPart & UriPart.Path) == UriPart.Path)
            {
                urlBuilder.Append(Request.Url.LocalPath);
            }
            if ((uriPart & UriPart.Query) == UriPart.Query)
            {
                urlBuilder.Append(Request.Url.Query);
            }
            return urlBuilder.ToString();
        }

        public string Method => Request?.HttpMethod;

        public bool ContainsHeader(string header)
        {
            return Request?.Headers.Get(header) != null;
        }

        public string GetHEader(string header) => Request?.Headers.Get(header);

        public string Ip => Request?.UserHostAddress;

        public string TryGetBodyContent()
        {
            if (Request == null || !(Request.InputStream.Length <= 0))
            {
                return null;
            }
            string documentContents = null;
            try
            {
                using (var stream = new MemoryStream())
                {
                    Request.InputStream.Seek(0, SeekOrigin.Begin);
                    Request.InputStream.CopyTo(stream);
                    documentContents = Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception)
            {
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
