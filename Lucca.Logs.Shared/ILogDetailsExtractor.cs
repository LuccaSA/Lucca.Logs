namespace Lucca.Logs.Shared
{
    public interface ILogDetailsExtractor
    {
        LogDetail CreateLogDetail(bool extractPayload);

    }
}