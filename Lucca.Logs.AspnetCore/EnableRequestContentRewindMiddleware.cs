using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
#if NETCOREAPP2_1
using Microsoft.AspNetCore.Http.Internal;
#endif

namespace Lucca.Logs.AspnetCore
{
    /// <summary>
    /// Middleware to enable HttpRequest Body content inspection
    /// </summary>
    public class EnableRequestContentRewindMiddleware
    {
        private readonly RequestDelegate _next;

        public EnableRequestContentRewindMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "always not null")]
        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();
            await _next(context).ConfigureAwait(false);
        }
    }
}