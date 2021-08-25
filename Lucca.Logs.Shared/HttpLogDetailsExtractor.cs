namespace Lucca.Logs.Shared
{
    public class HttpLogDetailsExtractor : ILogDetailsExtractor
    {
        private readonly IHttpContextParser _httpRequest;

        public HttpLogDetailsExtractor(IHttpContextParser httpRequest)
        {
            _httpRequest = httpRequest;
        }
          
         
        public LogDetail CreateLogDetail(bool extractPayload)
        {
            IHttpContextRequest httpRequest = _httpRequest.HttpRequestAccessor();
            if (httpRequest == null)
            {
                return new LogDetail { CanExtract = false };
            }

            return new LogDetail
            {
                CanExtract = true,
                PageRest = _httpRequest.ExtractUrl(Uripart.Path | Uripart.Query, httpRequest),
                PageRest2 = _httpRequest.ExtractUrl(Uripart.Path | Uripart.Query, httpRequest),
                Page = _httpRequest.ExtractUrl(Uripart.Full, httpRequest),
                Verb = _httpRequest.GetMethod(httpRequest),
                Uri = _httpRequest.ExtractUrl(Uripart.Path, httpRequest),
                ServerName = _httpRequest.ExtractUrl(Uripart.Host, httpRequest),
                HostAddress = _httpRequest.HostAddress(httpRequest),
                UserAgent = _httpRequest.GetHeader("User-Agent", httpRequest),
                CorrelationId = _httpRequest.GetHeader(LogMeta.CorrelationId, httpRequest),
                Payload = extractPayload ? _httpRequest.TryGetBodyContent(httpRequest) : null,
                Warning = _httpRequest == null ? "HttpContext.Current is null" : null
            };

        }
    }
}