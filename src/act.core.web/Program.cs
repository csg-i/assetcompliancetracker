using System;
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
                    var hostingEnvironment = hostingContext.HostingEnvironment;
                    var isProd = hostingEnvironment.IsProduction();
                    var disableAws = System.Environment.GetEnvironmentVariable("DISABLE_AWS");
                    if (isProd && !string.Equals(disableAws, "1", StringComparison.Ordinal))
                    {
                        var path = "Production";
                        builder.AddSystemsManager($"/ACT/{path}");
                    }
                })
                .ConfigureLogging((context, logging) =>
                {
                    var disableAws = string.Equals(Environment.GetEnvironmentVariable("DISABLE_AWS"), "1", StringComparison.Ordinal);
                    if (context.HostingEnvironment.IsProduction() && !disableAws)
                    {
                        logging.AddAWSProvider();
                    }
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
            
                .ConfigureKestrel(o => o.ConfigurationLoader.Load())
                .UseStartup<Startup>();



    }

}
