using System;
using System.Diagnostics;
using System.Threading.Tasks;
using act.core.data;
using act.core.etl;
using act.core.web.Framework;
using act.core.web.Models.AppSpecs;
using act.core.web.Models.OsSpecs;
using act.core.web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;

using Amazon.S3;
using Amazon.S3.Model;
using AspNetCore.DataProtection.Aws.S3;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace act.core.web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        class AdfsSection
        {
            public string MetadataAddress { get; set; }
            public string Wtrealm { get; set; }
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //setup adfs
            var adfs = new AdfsSection();
            _configuration.GetSection("ADFS").Bind(adfs);
            
            
            //register all dependencies
            services
                .AddDefaultAWSOptions(_configuration.GetAWSOptions())
                .AddActDbContextPool(_configuration)
                .ConfigureGatherer()              
                .AddTransient<ISpecificationFactory<OsSpecInformation, OsSpecSearchResult>, OsSpecificationFactory>()
                .AddTransient<ISpecificationFactory<AppSpecInformation, AppSpecSearchResult>, AppSpecificationFactory>()
                .AddTransient<IBuildSpecificationFactory, BuildSpecificationFactory>()
                .AddTransient<ISuggestionFactory, SuggestionFactory>()
                .AddTransient<IJustificationFactory, JustificationFactory>()
                .AddTransient<ISoftwareComponentFactory, SoftwareComponentFactory>()
                .AddTransient<IScoreCardFactory, ScoreCardFactory>()
                .AddTransient<IEmployeeFactory, EmployeeFactory>()
                .AddTransient<INodeFactory, NodeFactory>()
                .AddTransient<IPortFactory, PortFactory>()
                .AddTransient<IDashboardFactory, DashboardFactory>()
                .AddTransient<INotifier, Notifier>()
                .AddSingleton<IExcelExporter, ExcelExporter>()
                .AddAWSService<IAmazonS3>()
                .AddDataProtection()
                .SetApplicationName("ACT")
                .PersistKeysToAwsS3(_configuration)
                .Services
                .AddAuthentication(o =>
                {
                    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
                })
                .AddWsFederation(o =>
                {
                    o.MetadataAddress = adfs.MetadataAddress;
                    o.Wtrealm = adfs.Wtrealm;
                    o.Events.OnRedirectToIdentityProvider = context =>
                    {
                        if (context.Request.IsAjaxRequest())
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.HandleResponse();
                        }

                        return Task.CompletedTask;
                    };
                })
                .AddCookie(o =>
                {
                    o.Cookie.HttpOnly = true;
                    o.Cookie.Name = "ACT";
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(45);
                }).Services
                .Configure<MvcOptions>(o =>
                {
                    o.Filters.Add(new RequireHttpsAttribute());                     
                })
                .AddMvc(o => //require auth user by default
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    o.Filters.Add(new AuthorizeFilter(policy));
                });
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory
                .AddAWSProvider(_configuration.GetAWSLoggingConfigSection(),
                    (logLevel, message, ex) => $"[{DateTime.UtcNow}] {logLevel}: {message} {ex}");
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Health/Error/500");
            }
            
            app.UseStatusCodePages(async ctx =>
            {
                if (ctx.HttpContext.Request.IsAjaxRequest())
                {
                    if (ctx.HttpContext.Request.IsAjaxHtml())
                        ctx.HttpContext.Response.ContentType = "text/html";
                    else
                        ctx.HttpContext.Response.ContentType = "application/json";

                    await ctx.HttpContext.Response.WriteAsync("{}");
                }
                else
                {
                    ctx.HttpContext.Response.Redirect($"/Health/Error/{ctx.HttpContext.Response.StatusCode}");
                }
            });
            app.UseAuthentication();
            app.UseStaticFiles();
            var logger = loggerFactory.CreateLogger<Startup>();
                
            logger.LogInformation($"Startup of ACT Web Version {FileVersionInfo.GetVersionInfo(GetType().Assembly.Location).ProductVersion}");
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
