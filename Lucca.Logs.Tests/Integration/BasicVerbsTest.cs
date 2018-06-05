using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    public class Startup
    {
        private readonly ErrorStore _memoryErrorStore;

        public Startup(IConfiguration configuration, ErrorStore memoryErrorStore)
        {
            Configuration = configuration;
            this._memoryErrorStore = memoryErrorStore;
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddApplicationPart(typeof(DirectExceptionController).Assembly);

            services.AddLogging(l =>
            {
                l.AddLuccaLogs(o =>
                {

                }, _memoryErrorStore);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
            app.UseLuccaLogs();
        }
    }

    public class TestDto
    {
        public string Data { get; set; }
    }

    [Route("api/[controller]")]
    public class DirectExceptionController : Controller
    {
        private readonly ILoggerFactory _loggerFactory;

        public DirectExceptionController(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            try
            {
                throw new NotImplementedException("get");
            }
            catch (Exception e)
            {
                _loggerFactory.CreateLogger<DirectExceptionController>().LogError(e, "DirectExceptionController");
            }
            return Enumerable.Empty<string>();
        }

        [HttpPost]
        public void Post([FromBody] TestDto dto)
        {
            try
            {
                throw new NotImplementedException("post");
            }
            catch (Exception e)
            {
                _loggerFactory.CreateLogger<DirectExceptionController>().LogError(e, "DirectExceptionController");
            }
        }
    }
}
