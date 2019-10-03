using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    }
}
