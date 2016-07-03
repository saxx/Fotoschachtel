using Fotoschachtel.Services;
using Haufwerk.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fotoschachtel.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddEnvironmentVariables("Fotoschachtel:");

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<Settings>(settings => Configuration.GetSection("Settings").Bind(settings));

            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddHaufwerk("Fotoschachtel", "https://haufwerk.sachsenhofer.com");

            services.AddTransient<SasService>();
            services.AddTransient<MetadataService>();
            services.AddTransient<ThumbnailsService>();
            services.AddTransient<HashService>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();
            app.UseDeveloperExceptionPage();
            app.UseHaufwerk();
            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute("json_thumbnails_with_password", "json/event/{event}:{password}/thumbnails", new { controller = "Json", action = "RenderThumbnails" });
                routes.MapRoute("json_thumbnails", "json/event/{event}/thumbnails", new { controller = "Json", action = "RenderThumbnails", password = "" });
                routes.MapRoute("json_token_with_password", "json/event/{event}:{password}", new { controller = "Json", action = "GetStorageUrl" });
                routes.MapRoute("json_token", "json/event/{event}", new { controller = "Json", action = "GetStorageUrl", password = "" });
            });



        }
    }
}
