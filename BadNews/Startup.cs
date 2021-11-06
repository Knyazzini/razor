using BadNews.ModelBuilders.News;
using BadNews.Repositories.News;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using BadNews.Validation;

namespace BadNews
{
    public class Startup
    {
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration configuration;

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.env = env;
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var mvcBuilder = services.AddControllersWithViews();
            if (env.IsDevelopment())
                mvcBuilder.AddRazorRuntimeCompilation();
            services.AddSingleton<INewsRepository, NewsRepository>();
            services.AddSingleton<INewsModelBuilder, NewsModelBuilder>();
            services.AddSingleton<IValidationAttributeAdapterProvider, StopWordsAttributeAdapterProvider>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Errors/Exception");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSerilogRequestLogging();
            app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("status-code", "StatusCode/{code?}", new
                {
                    controller = "Errors",
                    action = "StatusCode"
                });
                endpoints.MapControllerRoute("default", "{controller=News}/{action=Index}/{id?}");
            });


        }
    }
}
