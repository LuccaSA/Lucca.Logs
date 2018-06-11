using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Lucca.Logs.Shared;

namespace Lucca.Logs.AspnetLegacy
{
    public class ExceptionHandler : IExceptionHandler
    {
        private void Handle(ExceptionHandlerContext context)
        {
            IExceptionalFilter efilter = context.RequestContext.Configuration.DependencyResolver.GetService(typeof(IExceptionalFilter)) as IExceptionalFilter;

            if (efilter == null)
            {
                return;
            }

            string message = efilter.IsInternalException(context.Exception) ? efilter.GenericErrorMessage : context.Exception.Message;

            context.Result = new TextPlainErrorResult
            {
                Request = context.ExceptionContext.Request,
                Content = message
            };
        }

        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Handle(context);
            return Task.CompletedTask;
        }
         
        private class TextPlainErrorResult : IHttpActionResult
        {
            public HttpRequestMessage Request { get; set; }

            public string Content { get; set; }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.Content = new StringContent(Content);
                response.RequestMessage = Request;
                return Task.FromResult(response);
            }
        }
    }
}