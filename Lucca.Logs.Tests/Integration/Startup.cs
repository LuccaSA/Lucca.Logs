using Lucca.Logs.AspnetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Exceptional;

namespace Lucca.Logs.Tests.Integration
{
    public class Startup
    {
        private readonly ErrorStore _memoryErrorStore;
        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IConfiguration configuration, ErrorStore memoryErrorStore, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            _memoryErrorStore = memoryErrorStore;
            _hostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddApplicationPart(typeof(DirectExceptionController).Assembly);

            services.AddLogging(l =>
            {
                l.AddLuccaLogs(_hostingEnvironment,o =>
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
}