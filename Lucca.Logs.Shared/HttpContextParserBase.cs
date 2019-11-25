using System;
using System.Collections.Generic;

namespace Lucca.Logs.Shared
{
    public abstract class HttpContextParserBase : IContextParser
    {
        public abstract string ExtractUrl(Uripart uriPart);
        public abstract string GetMethod();
        public abstract string Ip { get; }
        public abstract bool ContainsHeader(string header);
        public abstract string GetHeader(string header);
        public abstract string TryGetPayload();

        public abstract Guid? ExceptionalLog(Exception exception, Dictionary<string, string> customData, string categoryName, string appName);

        public abstract bool CanRead();

        public string GetClientIp()
        {
            string ip = null;
            if (ContainsHeader(LuccaDataWrapper._luccaForwardedHeader))
            {
                ip = GetHeader(LuccaDataWrapper._forwardedHeader);
            }
            if (string.IsNullOrEmpty(ip))
            {
                return Ip;
            }
            return ip;
        }

        public string GetCorrelationId()
        {
            if (ContainsHeader(LuccaDataWrapper._correlationId))
            {
                return GetHeader(LuccaDataWrapper._correlationId);
            }
            return null;
        }

        public string GetPage()
        {
            return ExtractUrl(Uripart.Full);
        }

        public string GetPageRest()
        {
            return ExtractUrl(Uripart.Path | Uripart.Query);
        }

        public string GetPageRest2()
        {
            return GetPageRest();
        }

        public string GetServerName()
        {
            return ExtractUrl(Uripart.Host);
        }

        public string GetUri()
        {
            return ExtractUrl(Uripart.Path);
        }

        public string GetUserAgent()
        {
            return GetHeader("User-Agent");
        }
    }
}