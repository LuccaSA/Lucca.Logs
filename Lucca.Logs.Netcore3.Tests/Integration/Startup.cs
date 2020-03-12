using Lucca.Logs.AspnetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Stores;

namespace Lucca.Logs.Netcore.Tests.Integration
{
    public class Startup
    {
        private readonly ErrorStore _memoryErrorStore;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _memoryErrorStore = new MemoryErrorStore(42);
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddApplicationPart(typeof(DirectExceptionController).Assembly);

            services.AddSingleton(_memoryErrorStore);

            services.AddLogging(l =>
            {
                l.AddLuccaLogs(o =>
                {

                }, "IntegrationTest", _memoryErrorStore);
            });
        }

#pragma warning disable CA1822 // Mark members as static
        public void Configure(IApplicationBuilder app)
#pragma warning restore CA1822 // Mark members as static
        {
            app.UseLuccaLogs();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}