using emby_exporter.Options;
using emby_exporter.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace emby_exporter
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<EmbyOptions>(_configuration.GetSection(EmbyOptions.SectionName));
            services.AddHttpClient<Scrapper>((services, client) =>
            {
                var embyOptions = services.GetService<IOptions<EmbyOptions>>();
                client.BaseAddress = new Uri(embyOptions.Value.Url);
                client.DefaultRequestHeaders.Add("X-Emby-Token", embyOptions.Value.Key);
            });
            Metrics.SuppressDefaultMetrics();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,Scrapper scrapper)
        {
            scrapper.Configure();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics();
            });
        }
    }
}
