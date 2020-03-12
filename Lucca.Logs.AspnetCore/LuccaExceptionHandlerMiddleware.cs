using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lucca.Logs.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Lucca.Logs.AspnetCore
{
    public class LuccaExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IExceptionQualifier _filter;
        private readonly ILogger _logger;
        private readonly IOptions<LuccaExceptionHandlerOption> _options;

        private const string _textPlain = "text/plain";
        private const string _textHtml = "text/html";
        private const string _appJson = "application/json";

        public LuccaExceptionHandlerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IExceptionQualifier filters, IOptions<LuccaExceptionHandlerOption> options)
        {
            _next = next;
            _filter = filters;
            _options = options;
            _logger = loggerFactory.CreateLogger<LuccaExceptionHandlerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
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
                    var info = new LuccaExceptionBuilderInfo(ex,
                        _filter?.StatusCode(ex) ?? HttpStatusCode.InternalServerError,
                        _filter?.DisplayExceptionDetails(ex) ?? false,
                        _filter?.GenericErrorMessage,
                        _filter?.PreferedResponseType(context.Request.Path));

                    await GenerateErrorResponseAsync(context, info).ConfigureAwait(false);
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

        private async Task GenerateErrorResponseAsync(HttpContext context, LuccaExceptionBuilderInfo info)
        {
            // Prepares the response request
            ClearHttpResponseResponse(context, info.StatusCode);

            // Extracts the Accept header
            string acceptable = NegociateAcceptableContentType(context.Request);

            if (acceptable != null)
            {
                if (await TryRenderErrorOnContentTypeAsync(acceptable, context, info).ConfigureAwait(false))
                {
                    return;
                }
            }

            // If explicit content type (Content-Type header) match, we render
            if (!string.IsNullOrEmpty(context.Request.ContentType) && await TryRenderErrorOnContentTypeAsync(context.Request.ContentType, context, info).ConfigureAwait(false))
            {
                return;
            }

            // fallback on PreferedResponseType
            if (!string.IsNullOrWhiteSpace(info.PreferedResponseType))
            {
                if (await TryRenderErrorOnContentTypeAsync(info.PreferedResponseType, context, info).ConfigureAwait(false))
                {
                    return;
                }
            }

            // fallback on generic text/plain error
            context.Response.ContentType = _textPlain;
            await context.Response.WriteAsync(info.GenericErrorMessage).ConfigureAwait(false);
        }

        internal static string NegociateAcceptableContentType(HttpRequest httpRequest)
        {
            var contentTypes = GetAcceptableMediaTypes(httpRequest);
            var acceptable = GetFirstAcceptableMatch(contentTypes);
            return acceptable;
        }

        private static string GetFirstAcceptableMatch(IEnumerable<MediaTypeHeaderValue> contentTypes)
        {
            if (contentTypes == null)
            {
                return null;
            }
            foreach (MediaTypeHeaderValue contentType in contentTypes)
            {
                if (!contentType.MediaType.HasValue)
                {
                    continue;
                }
                if (contentType.MediaType.Value.StartsWith(_textPlain, StringComparison.OrdinalIgnoreCase)
                    || contentType.MediaType.Value.StartsWith(_appJson, StringComparison.OrdinalIgnoreCase)
                    || contentType.MediaType.Value.StartsWith(_textHtml, StringComparison.OrdinalIgnoreCase))
                {
                    return contentType.MediaType.Value;
                }
            }
            return null;
        }

        private async Task<bool> TryRenderErrorOnContentTypeAsync(string contentType, HttpContext context, LuccaExceptionBuilderInfo info)
        {
            try
            {
                switch (contentType)
                {
                    case _textPlain:
                        await TextPlainReport(context, info).ConfigureAwait(false);
                        return true;
                    case _appJson:
                        await JsonReport(context, info).ConfigureAwait(false);
                        return true;
                    case _textHtml:
                        await HtmlReport(context, info).ConfigureAwait(false);
                        return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(0, e, "An exception was thrown attempting to render the error with content type " + contentType);
            }
            return false;
        }

        private async Task HtmlReport(HttpContext httpContext, LuccaExceptionBuilderInfo info)
        {
            string data = await _options.Value.HtmlResponse(info).ConfigureAwait(false);
            httpContext.Response.ContentType = _textHtml;
            await httpContext.Response.WriteAsync(data).ConfigureAwait(false);
        }

        private async Task TextPlainReport(HttpContext httpContext, LuccaExceptionBuilderInfo info)
        {
            string data = await _options.Value.TextPlainResponse(info).ConfigureAwait(false);
            httpContext.Response.ContentType = _textPlain;
            await httpContext.Response.WriteAsync(data).ConfigureAwait(false);
        }

        private async Task JsonReport(HttpContext httpContext, LuccaExceptionBuilderInfo info)
        {
            string data = await _options.Value.JsonResponse(info).ConfigureAwait(false);
            httpContext.Response.ContentType = _appJson;
            await httpContext.Response.WriteAsync(data).ConfigureAwait(false);
        }

        private static List<MediaTypeHeaderValue> GetAcceptableMediaTypes(HttpRequest request)
        {
            //https://developer.mozilla.org/en-US/docs/Glossary/Quality_values
            return request.GetTypedHeaders()?.Accept?.OrderByDescending(h => h.Quality ?? 1).ToList();
        }

        private static void ClearHttpResponseResponse(HttpContext context, HttpStatusCode statusCode)
        {
            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.OnStarting(ClearCacheHeadersCallbackAsync, context.Response);
        }

        private static Task ClearCacheHeadersCallbackAsync(object state)
        {
            var response = (HttpResponse)state;
            response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "-1";
            response.Headers.Remove(HeaderNames.ETag);
            return Task.CompletedTask;
        }
    }
}
