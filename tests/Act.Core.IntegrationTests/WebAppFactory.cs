using System.Linq;
using act.core.data;
using act.core.web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Act.Core.IntegrationTests
{
	public class WebAppFactory : WebApplicationFactory<Program>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureAppConfiguration((ctx, cfg) =>
			{
				// Use in-memory configuration to short-circuit AWS Systems Manager
				cfg.AddInMemoryCollection(new[]
				{
					new KeyValuePair<string, string>("InventorySystemLinkFormat", "https://inventory/{0}"),
					new KeyValuePair<string, string>("ADFS:MetadataAddress", "https://test/metadata"),
					new KeyValuePair<string, string>("ADFS:Wtrealm", "https://test/realm"),
					new KeyValuePair<string, string>("Mail:Host", "localhost"),
					new KeyValuePair<string, string>("Mail:Port", "25"),
					new KeyValuePair<string, string>("Mail:From", "test@example.com"),
				}
				);
			});

			builder.ConfigureServices(services =>
			{
				// Replace DB context with InMemory
				var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ActDbContext>));
				if (descriptor != null)
				{
					services.Remove(descriptor);
				}
				services.AddDbContext<ActDbContext>(o => o.UseInMemoryDatabase("act-tests"));

				// Build the provider and seed minimal data
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
			ctx.Environments.Add(new Environment
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