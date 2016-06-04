using System.IO;
using Fotoschachtel.Services;
using Fotoschachtel.ViewModels.Event;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            //services.AddHaufwerk("Fotoschachtel", "http://haufwerk.sachsenhofer.com");
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

            app.UseDeveloperExceptionPage();
            // app.UseHaufwerk();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute("json_thumbnails_with_password", "json/event/{eventId}:{password}/thumbnails", new { controller = "Json", action = "RenderThumbnails" });
                routes.MapRoute("json_thumbnails", "json/event/{eventId}/thumbnails", new {controller = "Json", action = "RenderThumbnails", password = ""});
                routes.MapRoute("json_thumbnail_with_password", "json/event/{eventId}:{password}/picture/{filename}/thumbnails", new { controller = "Json", action = "RenderThumbnail" });
                routes.MapRoute("json_thumbnail", "json/event/{eventId}/picture/{filename}/thumbnails", new { controller = "Json", action = "RenderThumbnail", password = "" });
                routes.MapRoute("json_token_with_password", "json/event/{eventId}:{password}", new { controller = "Json", action = "GetStorageUrl" });
                routes.MapRoute("json_token", "json/event/{eventId}", new { controller = "Json", action = "GetStorageUrl", password = "" });
            }
            );
        }
    }
}
