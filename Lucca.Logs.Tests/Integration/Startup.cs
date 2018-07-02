﻿using Lucca.Logs.AspnetCore;
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
}