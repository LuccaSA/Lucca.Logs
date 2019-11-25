using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Stores;
using Xunit;

namespace Lucca.Logs.Netcore3.Tests
{
    public class BasicVerbsTest
    {
        private readonly HttpClient _client;
        private readonly ErrorStore _memoryStore;

        public BasicVerbsTest()
        {
            _memoryStore = new MemoryErrorStore(42);

            var builder = new WebHostBuilder()
                .ConfigureServices(s => s.AddSingleton(_memoryStore))
                .UseStartup<Startup>();

            var testServer = new TestServer(builder);

            _client = testServer.CreateClient();
        }

        [Fact]
        public async Task ExOnGet()
        {
            var response = await _client.GetAsync("/api/directException");
            List<Error> found = await _memoryStore.GetAllAsync();

            response.EnsureSuccessStatusCode();

            Assert.Single(found);

            Assert.Equal("IntegrationTest", found.First().ApplicationName);
        }

        [Theory]
        [InlineData("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3", true)]
        [InlineData(null, true)]
        [InlineData("*/*", true)]
        [InlineData("application/json", true)]
        public async Task ExOnGetDirectWithAccept(string accept, bool jsonMode)
        {
            var client = _client;
            if (accept != null)
            {
                foreach (var v in accept.Split(","))
                {
                    if (MediaTypeWithQualityHeaderValue.TryParse(v, out var media))
                    {
                        client.DefaultRequestHeaders.Accept.Add(media);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            HttpResponseMessage response = await _client.GetAsync("/api/directException/direct");

            var data = await response.Content.ReadAsStringAsync();

            List<Error> found = await _memoryStore.GetAllAsync();

            Assert.NotEmpty(found);

            if (jsonMode)
            {
                Assert.StartsWith("{\"status\":500,\"message\":\"exception message\"}", data);
            }
            else
            {
                Assert.StartsWith("Oops ! ", data);
            }

            Assert.Equal("IntegrationTest", found.First().ApplicationName);
        }
    }
}
