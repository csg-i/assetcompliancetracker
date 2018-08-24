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
        private static Dictionary<string, string> ParseEbConfig(IConfiguration config)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (IConfigurationSection pair in config.GetSection("iis:env").GetChildren())
            {
                string[] keypair = pair.Value.Split(new[] { '=' }, 2);
                dict.Add(keypair[0], keypair[1]);
            }

            return dict;
        }
        private static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    if (!hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        config.AddJsonFile(@"C:\Program Files\Amazon\ElasticBeanstalk\config\containerconfiguration", true, true);
                        config.AddInMemoryCollection(ParseEbConfig(config.Build()));
                    }
                })
                .UseStartup<Startup>()
                .UseKestrel(o => o.ConfigureEndpoints())
                .UseIISIntegration()
                .Build();

  
    }

}
