using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lucca.Logs.AspnetCore
{
    public class LuccaExceptionHandlerOption
    {
        public LuccaExceptionHandlerOption()
        {
            TextPlainResponse = i => Task.FromResult("Error code: " + i.StatusCode);

            JsonResponse = i => Task.FromResult(JsonConvert.SerializeObject(new
            {
                status = i.StatusCode,
                message = i.Exception.Message,
                data = new { },
                stackTrace = i.Exception.StackTrace
            }));

            HtmlResponse = i => throw new NotImplementedException();
        }

        public Func<LuccaExceptionBuilderInfo, Task<String>> TextPlainResponse { get; set; }
        public Func<LuccaExceptionBuilderInfo, Task<String>> HtmlResponse { get; set; }
        public Func<LuccaExceptionBuilderInfo, Task<String>> JsonResponse { get; set; }
    }

    public class LuccaExceptionBuilderInfo
    {
        public LuccaExceptionBuilderInfo(Exception exception, HttpStatusCode statusCode, bool displayExceptionDetails, string genericErrorMessage)
        {
            Exception = exception;
            StatusCode = statusCode;
            DisplayExceptionDetails = displayExceptionDetails;
            GenericErrorMessage = genericErrorMessage;
        }

        public Exception Exception { get; }
        public HttpStatusCode StatusCode { get; }
        public bool DisplayExceptionDetails { get; }
        public string GenericErrorMessage { get; }

    }
}