namespace Lucca.Logs.Shared
{
    public interface ILogDetailsExtractor
    {
        LogDetail CreateLogDetail();
    }

    public class LogDetail
    {
        public static LogDetail NoExtraction = new() { CanExtract = false };

        public bool CanExtract { get; set; }
        public string PageRest { get; set; }
        public string PageRest2 { get; set; }
        public string Page { get; set; }
        public string Verb { get; set; }
        public string Uri { get; set; }
        public string ServerName { get; set; }
        public string HostAddress { get; set; }
        public string UserAgent { get; set; }
        public string CorrelationId { get; set; }
        public string Payload { get; set; }
        public string Warning { get; set; }

        public string XForwardedFor { get; set; }
        public string CFConnectingIP { get; set; }
        public string CFRAY { get; set; }
    }
}