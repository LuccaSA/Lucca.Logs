using Lucca.Logs.AspnetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Exceptional;

namespace Lucca.Logs.Netcore3.Tests
{
    public class Startup
    {
        private readonly ErrorStore _memoryErrorStore;

        public Startup(IConfiguration configuration, ErrorStore memoryErrorStore)
        {
            Configuration = configuration;
            _memoryErrorStore = memoryErrorStore;
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

                }, "IntegrationTest", _memoryErrorStore);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseLuccaLogs();
        }
    }
}