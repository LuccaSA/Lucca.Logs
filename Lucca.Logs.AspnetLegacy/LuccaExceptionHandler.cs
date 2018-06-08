using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace Lucca.Logs.AspnetLegacy
{
    public class LuccaExceptionHandler : IExceptionHandler
    {
        public void Handle(ExceptionHandlerContext context)
        {
            //int status = 0;
            //if (context.Exception is DomainException)
            //{
            //    status = (int)((DomainException)context.Exception).Status;
            //}
            //else if (context.Exception is APIException)
            //{
            //    status = (int)((APIException)context.Exception).Status;
            //}

            //context.Result = new TextPlainErrorResult
            //{
            //    Request = context.ExceptionContext.Request,
            //    Content = status >= 400 && status < 500 ? context.Exception.Message : Lucca.Web.Sdk.Exceptions.JsonExceptionAttribute.ErrorMessage
            //};
        }

        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Handle(context);
            return Task.CompletedTask;
        }

        public bool ShouldHandle(ExceptionHandlerContext context)
        {
            return true;
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