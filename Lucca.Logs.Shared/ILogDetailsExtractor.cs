namespace Lucca.Logs.Abstractions
{
    public interface ILogDetailsExtractor
    {
        bool CanExtract { get; }
        string PageRest { get; }
        string PageRest2 { get; }
        string Page { get; }
        string Verb { get; }
        string UriPath { get; }
        string ServerName { get; }
        string HostAddress { get; }
        string UserAgent { get; }
        string CorrelationId { get; }
        string Payload { get; }
        string Warning { get; }
    }
}