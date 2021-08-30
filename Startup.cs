using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace netcore_authentication
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("MyApp").AddCookie("MyApp", op => 
            {
                op.Cookie.Name = "Ticket";
                op.Cookie.HttpOnly = true;
                op.ExpireTimeSpan = TimeSpan.FromSeconds(15);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
                endpoints.MapGet("/login/{username}", async context =>
                {
                    var username = context.GetRouteValue("username").ToString();

                    var identity = new ClaimsIdentity("MyApp");
                    identity.AddClaim(new Claim(ClaimTypes.Name, username));
                    identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));

                    var principal = new ClaimsPrincipal(identity);
                    await context.SignInAsync("MyApp", principal);
                    await context.Response.WriteAsync("Logged in!");
                });
                endpoints.MapGet("/private", async context =>
                {
                    if (context.User.Identity.IsAuthenticated) 
                    {
                        await context.Response.WriteAsync("Secret content");
                    }
                    else
                    {
                        await context.Response.WriteAsync("Not allowed");
                    }
                });
                endpoints.MapGet("/logout", async context =>
                {
                    await context.SignOutAsync("MyApp");
                    await context.Response.WriteAsync("Logged out");
                });
            });
        }
    }
}
