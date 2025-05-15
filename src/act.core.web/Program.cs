using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace act.core.web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Build().Run();
        }

        private static IWebHostBuilder BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var hostingEnvironment = hostingContext.HostingEnvironment;
                    var path = hostingEnvironment.IsProduction() ? "Production" : "NonProd";
                    builder.AddSystemsManager($"/act/{path}");
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddAWSProvider();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
            
                .ConfigureKestrel(o => o.ConfigurationLoader.Load())
                .UseStartup<Startup>();



    }

}
