using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Lucca.Logs.AspnetCore
{
    public static class LuccaLoggerAspnetCoreExtensions
    {
        public static IApplicationBuilder UseLuccaLogs(this IApplicationBuilder app, LuccaExceptionHandlerOption? exceptionHandlerOption = null, bool enableContentLog = true)
        {
            var builder = app;
            if (enableContentLog)
            {
                builder = builder.UseMiddleware<EnableRequestContentRewindMiddleware>();
            }

            app.UseMiddleware<LuccaExceptionHandlerMiddleware>(Options.Create(exceptionHandlerOption ?? new LuccaExceptionHandlerOption()));

            return builder;
        }
    }
}