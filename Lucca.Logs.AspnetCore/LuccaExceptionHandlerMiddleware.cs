using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lucca.Logs.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Lucca.Logs.AspnetCore
{
    public class LuccaExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IExceptionQualifier _filters;
        private readonly ILogger _logger;
        private readonly IOptions<LuccaExceptionHandlerOption> _options;

        public LuccaExceptionHandlerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory,
            IExceptionQualifier filters, IOptions<LuccaExceptionHandlerOption> options)
        {
            _next = next;
            _filters = filters;
            _options = options;
            _logger = loggerFactory.CreateLogger<LuccaExceptionHandlerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "An unhandled exception has occurred: " + ex.Message);
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("The response has already started, the error handler will not be executed.");
                    throw;
                }
                PathString originalPath = context.Request.Path;
                try
                {
                    context.Response.Clear();
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.OnStarting(ClearCacheHeaders, context.Response);

                    var info = new LuccaExceptionBuilderInfo(ex,
                            (HttpStatusCode)context.Response.StatusCode,
                            _filters?.DisplayExceptionDetails(ex) ?? false,
                            _filters?.GenericErrorMessage);

                    await ProcessAnswer(context, info);

                }
                catch (Exception ex2)
                {
                    _logger.LogError(0, ex2, "An exception was thrown attempting to execute the error handler.");
                    throw;
                }
                finally
                {
                    context.Request.Path = originalPath;
                }
            }
        }

        private async Task ProcessAnswer(HttpContext context, LuccaExceptionBuilderInfo info)
        {
            foreach (var mediaTypeSegmentWithQuality in GetAcceptableMediaTypes(context.Request))
            {
                switch (mediaTypeSegmentWithQuality.MediaType.ToString())
                {
                    case "text/plain":
                        await TextPlainReport(context, info);
                        return;
                    case "application/json":
                        await JsonReport(context, info);
                        return;
                    case "text/html":
                        await HtmlReport(context, info);
                        return;
                }
            }
            await TextPlainReport(context, info);
        }

        private async Task HtmlReport(HttpContext httpContext, LuccaExceptionBuilderInfo info)
        {
            string data = await _options.Value.HtmlResponse(info);
            httpContext.Response.ContentType = "text/html";
            await httpContext.Response.WriteAsync(data);
        }

        private async Task TextPlainReport(HttpContext httpContext, LuccaExceptionBuilderInfo info)
        {
            string data = await _options.Value.TextPlainResponse(info);
            httpContext.Response.ContentType = "text/plain";
            await httpContext.Response.WriteAsync(data);
        }

        private async Task JsonReport(HttpContext httpContext, LuccaExceptionBuilderInfo info)
        {
            string data = await _options.Value.JsonResponse(info);
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(data);
        }

        private List<MediaTypeSegmentWithQuality> GetAcceptableMediaTypes(HttpRequest request)
        {
            var result = new List<MediaTypeSegmentWithQuality>();
            AcceptHeaderParser.ParseAcceptHeader(request.Headers[HeaderNames.Accept], result);
            result.Sort(QualityComparer);
            return result;
        }
         
        private static int QualityComparer(MediaTypeSegmentWithQuality left, MediaTypeSegmentWithQuality right)
        {
            if (left.Quality > right.Quality)
                return -1;
            if (left.Quality == right.Quality)
                return 0;
            return 1;
        }

        private static Task ClearCacheHeaders(object state)
        {
            var response = (HttpResponse)state;
            response.Headers[HeaderNames.CacheControl] = "no-cache";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "-1";
            response.Headers.Remove(HeaderNames.ETag);
            return Task.CompletedTask;
        }
    }
}
