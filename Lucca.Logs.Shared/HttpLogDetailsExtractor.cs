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
                PageRest = httpRequest.ExtractUrl(Uriparts.Path | Uriparts.Query),
                PageRest2 = httpRequest.ExtractUrl(Uriparts.Path | Uriparts.Query),
                Page = httpRequest.ExtractUrl(Uriparts.Full),
                Verb = httpRequest.GetMethod(),
                Uri = httpRequest.ExtractUrl(Uriparts.Path),
                ServerName = httpRequest.ExtractUrl(Uriparts.Host),
                HostAddress = httpRequest.HostAddress(),
                UserAgent = httpRequest.GetHeader("User-Agent"),
                CorrelationId = httpRequest.GetHeader(LogMeta._correlationId),
                Payload = httpRequest.TryGetBodyContent()
            };

        }
    }
}