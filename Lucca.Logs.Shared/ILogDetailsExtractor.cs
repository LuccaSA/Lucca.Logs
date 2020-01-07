namespace Lucca.Logs.Shared
{
    public interface ILogDetailsExtractor
    {
        bool CanExtract { get; }
        string PageRest { get; }
        string PageRest2 { get; }
        string Page { get; }
        string Verb { get; }
        string Uri { get; }
        string ServerName { get; }
        string HostAddress { get; }
        string UserAgent { get; }
        string CorrelationId { get; }
        string Payload { get; }
        string Warning { get; }
        int Priority { get; }
    }
}