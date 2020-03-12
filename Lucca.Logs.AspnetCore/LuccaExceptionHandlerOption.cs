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
            TextPlainResponse = DefaultHandler;

            JsonResponse = i => Task.FromResult(JsonConvert.SerializeObject(new
            {
                status = i.StatusCode,
                message = i.Exception.Message
            }));

            HtmlResponse = i => throw new NotImplementedException();
        }

        public Func<LuccaExceptionBuilderInfo, Task<string>> TextPlainResponse { get; set; }
        public Func<LuccaExceptionBuilderInfo, Task<string>> HtmlResponse { get; set; }
        public Func<LuccaExceptionBuilderInfo, Task<string>> JsonResponse { get; set; }

        public static Task<string> DefaultHandler(LuccaExceptionBuilderInfo builderInfo) => Task.FromResult($"Error code: {builderInfo?.StatusCode}");
    }

    public class LuccaExceptionBuilderInfo
    {
        public LuccaExceptionBuilderInfo(Exception exception, HttpStatusCode statusCode, bool displayExceptionDetails, string genericErrorMessage, string preferedResponseType)
        {
            Exception = exception;
            StatusCode = statusCode;
            DisplayExceptionDetails = displayExceptionDetails;
            GenericErrorMessage = genericErrorMessage;
            PreferedResponseType = preferedResponseType;
        }

        public Exception Exception { get; }
        public HttpStatusCode StatusCode { get; }
        public bool DisplayExceptionDetails { get; }
        public string GenericErrorMessage { get; }
        public string PreferedResponseType { get; }
    }
}