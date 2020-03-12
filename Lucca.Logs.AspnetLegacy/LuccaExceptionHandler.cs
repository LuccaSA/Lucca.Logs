using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Lucca.Logs.Abstractions;

namespace Lucca.Logs.AspnetLegacy
{
    public class ExceptionHandler : IExceptionHandler
    {
        private void Handle(ExceptionHandlerContext context)
        {
            if (!(context.RequestContext.Configuration.DependencyResolver.GetService(typeof(IExceptionQualifier)) is IExceptionQualifier filter))
            {
                return;
            }

            string message = filter.DisplayExceptionDetails(context.Exception) ? context.Exception.Message : filter.GenericErrorMessage;

            context.Result = new TextPlainErrorResult
            {
                Request = context.ExceptionContext.Request,
                Content = message
            };
        }

        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            if (context != null)
            {
                Handle(context);
            }
            return Task.CompletedTask;
        }

        private class TextPlainErrorResult : IHttpActionResult
        {
            public HttpRequestMessage Request { get; set; }

            public string Content { get; set; }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(Content),
                    RequestMessage = Request
                };
                return Task.FromResult(response);
            }
        }
    }
}