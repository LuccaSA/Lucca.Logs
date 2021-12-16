namespace Lucca.Logs.Shared
{
    public class HttpLogDetailsExtractor : ILogDetailsExtractor
    {
        private readonly IHttpContextParser _httpRequest;

        public HttpLogDetailsExtractor(IHttpContextParser httpRequest)
        {
            _httpRequest = httpRequest;
        }
          
         
        public LogDetail CreateLogDetail()
        {
            IHttpContextRequest? httpRequest = _httpRequest.HttpRequestAccessor();
            if (httpRequest is null)
            {
                return new LogDetail
                {
                    CanExtract = false,
                    Warning = "HttpContext.Current is null"
                };
            }

            return new LogDetail
            {
                CanExtract = true,
                PageRest = httpRequest.ExtractUrl(Uripart.Path | Uripart.Query),
                PageRest2 = httpRequest.ExtractUrl(Uripart.Path | Uripart.Query),
                Page = httpRequest.ExtractUrl(Uripart.Full),
                Verb = httpRequest.GetMethod(),
                Uri = httpRequest.ExtractUrl(Uripart.Path),
                ServerName = httpRequest.ExtractUrl(Uripart.Host),
                HostAddress = httpRequest.HostAddress(),
                UserAgent = httpRequest.GetHeader("User-Agent"),
                CorrelationId = httpRequest.GetHeader(LogMeta._correlationId),
                Payload = httpRequest.TryGetBodyContent()
            };

        }
    }
}