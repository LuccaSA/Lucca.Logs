using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace Lucca.Logs
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

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableRewind();
            await _next(context);
        }
    }
}