using Microsoft.AspNetCore.Builder;

namespace Lucca.Logs.AspnetCore
{
    public static class LuccaLoggerAspnetCoreExtensions
    {
        public static IApplicationBuilder UseLuccaLogs(this IApplicationBuilder app, bool enableContentLog = true)
        {
            var builder = app;
            if (enableContentLog)
            {
                builder = builder.UseMiddleware<EnableRequestContentRewindMiddleware>();
            }
            return builder;
        }
    }
}