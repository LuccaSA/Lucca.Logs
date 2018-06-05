using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lucca.Logs.Shared;
using Microsoft.AspNetCore.Http;
using StackExchange.Exceptional;

namespace Lucca.Logs.AspnetCore
{
    public sealed class HttpContextCore : IHttpContextWrapper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextCore(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpRequest Request => _httpContextAccessor?.HttpContext?.Request;

        public string ExtractUrl(UriPart uriPart)
        {
            if (Request == null)
                return null;

            var urlBuilder = new StringBuilder();
            if ((uriPart & UriPart.Scheme) == UriPart.Scheme && !String.IsNullOrWhiteSpace(Request.Scheme))
            {
                urlBuilder.Append(Request.Scheme + "://");
            }
            if ((uriPart & UriPart.Host) == UriPart.Host)
            {
                urlBuilder.Append(Request.Host.Host);
            }
            if ((uriPart & UriPart.Port) == UriPart.Port && Request.Host.Port > 0)
            {
                urlBuilder.Append(":" + Request.Host.Port);
            }
            if ((uriPart & UriPart.Path) == UriPart.Path)
            {
                urlBuilder.Append(Request.PathBase.ToUriComponent());
                urlBuilder.Append(Request.Path.ToUriComponent());
            }
            if ((uriPart & UriPart.Query) == UriPart.Query)
            {
                urlBuilder.Append(Request.QueryString.Value);
            }
            return urlBuilder.ToString();
        }

        public string Method => Request?.Method;

        public bool ContainsHeader(string header)
        {
            if (Request == null)
                return false;

            return Request.Headers.ContainsKey(header);
        }

        public string GetHEader(string header) => Request?.Headers[header];

        public string Ip => Request?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string TryGetBodyContent()
        {
            if (Request == null || !Request.Body.CanRead || !Request.Body.CanSeek)
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

    }
}
