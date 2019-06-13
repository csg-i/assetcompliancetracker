using System.Collections.Generic;
using act.core.web.Framework;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace act.core.web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }
       
        private static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration( (hostingContext,builder) =>
                {
                    var hostingEnvironment = hostingContext.HostingEnvironment;
                    var path = hostingEnvironment.IsProduction() ? "Production" : "NonProd";
                    builder.AddSystemsManager($"/ACT/{path}");
                })
                .UseStartup<Startup>()
                .Build();

  
    }

}
