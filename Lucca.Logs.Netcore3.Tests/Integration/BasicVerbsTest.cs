using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using StackExchange.Exceptional;
using Xunit;

namespace Lucca.Logs.Netcore.Tests.Integration
{
    public class BasicVerbsTest
    {
        private readonly IHostBuilder _hostBuilder;
        public BasicVerbsTest()
        {
            _hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    // Add TestServer
                    webHost.UseTestServer();
                    webHost.UseStartup<Startup>();
                }); 
        }
         
        [Fact]
        public async Task ExOnGet()
        {
            var host = await _hostBuilder.StartAsync();
            var client = host.GetTestClient();
            var response = await client.GetAsync("/api/directException");

            var memoryStore = host.Services.GetRequiredService<ErrorStore>(); 
            List<Error> found = await memoryStore.GetAllAsync();

            response.EnsureSuccessStatusCode();
             
            Assert.Single(found);

            Assert.Equal("IntegrationTest", found.First().ApplicationName);
        }

        [Fact]
        public async Task ExOnPost()
        {
            var host = await _hostBuilder.StartAsync();
            var client = host.GetTestClient();
            TestDto dto = new TestDto { Data = "hey !" };
            var json = JsonConvert.SerializeObject(dto);

            HttpContent payload = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/directException", payload);
            response.EnsureSuccessStatusCode();

            var memoryStore = host.Services.GetRequiredService<ErrorStore>();
            List<Error> found = await memoryStore.GetAllAsync();

            Assert.Single(found);
            Assert.Contains("RawPostedData", found.First().CustomData.Keys);
            Assert.Equal(@"{""Data"":""hey !""}", found.First().CustomData["RawPostedData"]);
        }

        [Fact]
        public async Task ExOnGetDirectWithAccept()
        {
            var host = await _hostBuilder.StartAsync();
            var client = host.GetTestClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            HttpResponseMessage response = await client.GetAsync("/api/directException/direct");

            var data = await response.Content.ReadAsStringAsync();

            var memoryStore = host.Services.GetRequiredService<ErrorStore>();
            List<Error> found = await memoryStore.GetAllAsync();

            Assert.Single(found);
            Assert.StartsWith("{\"status\":500,\"message\":\"get\"}", data);

            Assert.Equal("IntegrationTest", found.First().ApplicationName);
        }
    }
}
