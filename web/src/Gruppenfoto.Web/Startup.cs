using Gruppenfoto.Web;
using Gruppenfoto.Web.ViewModels.Event;
using Haufwerk.Client;
using JetBrains.Annotations;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GruppenFoto.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables("Gruppenfoto:");
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }


        public void ConfigureServices([NotNull] IServiceCollection services)
        {
            services.AddHaufwerk("Gruppenfoto.Web", "http://haufwerk.sachsenhofer.com");
            services.AddTransient<IndexViewModel>();
            services.AddTransient<PictureStorage>();
            services.AddMvc();
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseHaufwerk();
            app.UseIISPlatformHandler();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
