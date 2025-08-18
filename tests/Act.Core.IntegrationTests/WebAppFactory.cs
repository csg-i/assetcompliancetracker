using System.Linq;
using System.Collections.Generic;
using act.core.data;
using act.core.web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Act.Core.IntegrationTests
{
	public class WebAppFactory : WebApplicationFactory<Program>
	{
		public WebAppFactory()
		{
			System.Environment.SetEnvironmentVariable("AWS_EC2_METADATA_DISABLED", "true");
			System.Environment.SetEnvironmentVariable("DISABLE_AWS", "1");
		}

		protected override IWebHostBuilder CreateWebHostBuilder()
		{
			// Build a minimal host to avoid Program.BuildWebHost and any external providers
			return new WebHostBuilder()
				.UseEnvironment("Development")
				.ConfigureAppConfiguration((ctx, cfg) =>
				{
					cfg.Sources.Clear();
					cfg.AddInMemoryCollection(new KeyValuePair<string, string?>[]
					{
						new KeyValuePair<string, string?>("InventorySystemLinkFormat", "https://inventory/{0}"),
						new KeyValuePair<string, string?>("ADFS:MetadataAddress", "https://test/metadata"),
						new KeyValuePair<string, string?>("ADFS:Wtrealm", "https://test/realm"),
						new KeyValuePair<string, string?>("Mail:Host", "localhost"),
						new KeyValuePair<string, string?>("Mail:Port", "25"),
						new KeyValuePair<string, string?>("Mail:From", "test@example.com"),
						new KeyValuePair<string, string?>("DataProtection:PersistTo", "Ephemeral"),
					});
				})
				.UseStartup<Startup>();
		}

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			System.Environment.SetEnvironmentVariable("AWS_EC2_METADATA_DISABLED", "true");
			System.Environment.SetEnvironmentVariable("DISABLE_AWS", "1");

			builder.ConfigureServices(services =>
			{
				services.PostConfigure<MvcOptions>(o =>
				{
					var httpsFilter = o.Filters.OfType<RequireHttpsAttribute>().FirstOrDefault();
					if (httpsFilter != null)
						o.Filters.Remove(httpsFilter);
				});

				var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ActDbContext>));
				if (descriptor != null)
				{
					services.Remove(descriptor);
				}
				services.AddDbContext<ActDbContext>(o => o.UseInMemoryDatabase("act-tests"));

				var sp = services.BuildServiceProvider();
				using var scope = sp.CreateScope();
				var ctx = scope.ServiceProvider.GetRequiredService<ActDbContext>();
				ctx.Database.EnsureCreated();
				TestSeeder.Seed(ctx);
			});
		}
	}

	internal static class TestSeeder
	{
		public static void Seed(ActDbContext ctx)
		{
			ctx.Environments.Add(new act.core.data.Environment
			{
				Id = 1,
				Name = "NonProd",
				Description = "np",
				ChefAutomateUrl = "https://chef.example.com",
				ChefAutomateOrg = "org",
				ChefAutomateToken = "token",
				Color = "#000000"
			});
			ctx.SaveChanges();
		}
	}
}