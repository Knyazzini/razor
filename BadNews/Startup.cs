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
using BadNews.Repositories.Weather;
using BadNews.Elevation;
using System;
using Microsoft.AspNetCore.Http;

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
            services.AddSingleton<INewsRepository, NewsRepository>();
            services.AddSingleton<INewsModelBuilder, NewsModelBuilder>();
            services.AddSingleton<IValidationAttributeAdapterProvider, StopWordsAttributeAdapterProvider>();
            services.AddSingleton<IWeatherForecastRepository, WeatherForecastRepository>();
            services.Configure<OpenWeatherOptions>(configuration.GetSection("OpenWeather"));
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });
            var mvcBuilder = services.AddControllersWithViews();
            if (env.IsDevelopment())
            {
                mvcBuilder.AddRazorRuntimeCompilation();

            }
        }

        public void Configure(IApplicationBuilder app)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Errors/Exception");
            }
            app.UseMiddleware<ElevationMiddleware>();
            app.UseHttpsRedirection();
            app.UseResponseCompression();
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = options =>
                {
                    options.Context.Response.GetTypedHeaders().CacheControl =
                        new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                        {
                            Public = false,
                            MaxAge = TimeSpan.FromDays(1)
                        };
                }
            });
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

            app.MapWhen(context => context.Request.IsElevated(), branchApp =>
            {
                branchApp.UseDirectoryBrowser("/files");
            });
        }
    }
}
