using Lucca.Logs.AspnetCore;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Lucca.Logs.Netcore.Tests
{

    public class ContentTypeTest
    {
        [Theory]
        [InlineData(null,null)]
        [InlineData("",null)]
        [InlineData("  ",null)]
        [InlineData("application/json", "application/json")]
        [InlineData("application/vnd+json", null)]
        [InlineData("text/html, application/xhtml+xml, application/xml;q=0.9, */*;q=0.8", "text/html")]
        public void EmptyContentType(string contentType, string expected)
        {
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
            httpContext.Request.Headers["Accept"] = contentType;

            var nego = LuccaExceptionHandlerMiddleware.NegociateAcceptableContentType(httpContext.Request);

            Assert.Equal(expected, nego);
        }
    }
}
