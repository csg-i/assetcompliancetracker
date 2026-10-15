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
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureAppConfiguration((hostingContext, builder) =>
                        {
                            var hostingEnvironment = hostingContext.HostingEnvironment;
                            var path = hostingEnvironment.IsProduction() ? "Production" : "NonProd";
                            builder.AddSystemsManager($"/ACT/{path}");
                            // Local smoke only: SSM /ACT/NonProd may override ActDB with a shared RDS.
                            // Re-apply local files + env last so Server=127.0.0.1 (SSH tunnel) wins.
                            if (hostingEnvironment.IsDevelopment())
                            {
                                builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                                builder.AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                                builder.AddEnvironmentVariables();
                            }
                        })
                        .ConfigureLogging((context, logging) =>
                        {
                            logging.AddAWSProvider();
                            logging.SetMinimumLevel(LogLevel.Debug);
                        })
                        .ConfigureKestrel(o => o.ConfigurationLoader.Load())
                        .UseStartup<Startup>();
                });
    }
}
