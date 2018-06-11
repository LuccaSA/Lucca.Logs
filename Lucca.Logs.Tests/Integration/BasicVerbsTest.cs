using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Stores;
using Xunit;

namespace Lucca.Logs.Tests.Integration
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
        }

        [Fact]
        public async Task ExOnPost()
        {
            TestDto dto = new TestDto { Data = "hey !" };
            var json = JsonConvert.SerializeObject(dto);

            HttpContent payload = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/directException", payload);
            response.EnsureSuccessStatusCode();
            List<Error> found = await _memoryStore.GetAllAsync();

            Assert.Single(found);
            Assert.Contains("RawPostedData", found.First().CustomData.Keys);
            Assert.Equal(@"{""Data"":""hey !""}", found.First().CustomData["RawPostedData"]);
        }
    }
}
