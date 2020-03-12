﻿namespace Lucca.Logs.Abstractions
{
    public class HttpLogDetailsExtractor : ILogDetailsExtractor
    {
        private readonly IHttpContextParser _httpRequest;

        public HttpLogDetailsExtractor(IHttpContextParser httpRequest)
        {
            _httpRequest = httpRequest;
        }

        public bool CanExtract => _httpRequest.IsContextDefined;
        public string PageRest => _httpRequest.ExtractUrl(UriParts.Path | UriParts.Query);
        public string PageRest2 => _httpRequest.ExtractUrl(UriParts.Path | UriParts.Query);
        public string Page => _httpRequest.ExtractUrl(UriParts.Full);
        public string Verb => _httpRequest.Method;
        public string UriPath => _httpRequest.ExtractUrl(UriParts.Path);
        public string ServerName => _httpRequest.ExtractUrl(UriParts.Host);

        public string HostAddress
        {
            get
            {
                // Récupération de l'IP forwardée par HAProxy, et fallback UserHostAddress
                string ip = null;
                if (_httpRequest.ContainsHeader(LogMeta._luccaForwardedHeader))
                {
                    ip = _httpRequest.GetHeader(LogMeta._forwardedHeader);
                }
                if (string.IsNullOrEmpty(ip))
                {
                    ip = _httpRequest.Ip;
                }
                return ip;
            }
        }

        public string UserAgent => _httpRequest.GetHeader("User-Agent");
        public string CorrelationId => _httpRequest.GetHeader(LogMeta._correlationId);
        public string Payload => _httpRequest.TryGetBodyContent();
        public string Warning => _httpRequest == null ? "HttpContext.Current is null" : null;
    }
}