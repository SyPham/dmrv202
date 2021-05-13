using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DMR_API._Repositories.Interface;
using DMR_API._Repositories.Repositories;
using DMR_API._Services.Interface;
using DMR_API._Services.Services;
using DMR_API.Data;
using DMR_API.Helpers;
using DMR_API.Helpers.AutoMapper;
using DMR_API.SignalrHub;
using DMR_API._Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Quartz;
using DMR_API.SchedulerHelper;
using DMR_API.SchedulerHelper.Jobs;
using DMR_API.SignalrHub.Client;
using System.Collections.Specialized;
using Microsoft.AspNetCore.CookiePolicy;
using DMR_API.Helpers.Extensions;
using dmr_api.Data;

namespace DMR_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var appsettings = Configuration.GetSection("Appsettings").Get<Appsettings>();

            services.AddDatabaseExention(Configuration)
                    .AddRepositoriesExention()
                    .AddServicesExention();

            services.AddSignalR();

            services.AddLogging();
          
            services.AddControllers()
                    .AddNewtonsoftJson(options =>
                                    {
                                        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
                                        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                                    });
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins(appsettings.CorsPolicy
                    ) //register for client
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            services.AddTransient<DbInitializer>();
            //Auto Mapper
            services.AddAutoMapperExention();

            services.AddAuthenticationWithSwaggerExention(Configuration);

            services.AddHttpClientExention(Configuration);

            services.AddShedulerExention();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataContext dataContext)
        {

            dataContext.Database.Migrate();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Electronic Scale");
            });
            app.UseCors("CorsPolicy");
            app.UseCors(x => x
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           //    .SetIsOriginAllowed(origin => true) // allow any origin
                           .AllowCredentials()); // allow credentials
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthentication()
               .UseCookiePolicy(new CookiePolicyOptions
               {
                    HttpOnly = HttpOnlyPolicy.Always
               });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ECHub>("/ec-hub");

            });
        }
    }
}
