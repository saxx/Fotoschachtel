using System.IO;
using Fotoschachtel.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Haufwerk.Client;

namespace Fotoschachtel
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();
            host.Run();
        }


        public IConfigurationRoot Configuration { get; }
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.local.json", optional: true)
                .AddEnvironmentVariables("Fotoschachtel:");
            Configuration = builder.Build();
        }


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


        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();
            app.UseDeveloperExceptionPage();
            app.UseHaufwerk();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute("json_thumbnails_with_password", "json/event/{event}:{password}/thumbnails", new { controller = "Json", action = "RenderThumbnails" });
                routes.MapRoute("json_thumbnails", "json/event/{event}/thumbnails", new {controller = "Json", action = "RenderThumbnails", password = ""});
                routes.MapRoute("json_token_with_password", "json/event/{event}:{password}", new { controller = "Json", action = "GetStorageUrl" });
                routes.MapRoute("json_token", "json/event/{event}", new { controller = "Json", action = "GetStorageUrl", password = "" });
            }
            );
        }
    }
}
