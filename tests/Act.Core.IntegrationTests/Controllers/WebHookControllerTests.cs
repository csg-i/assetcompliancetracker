using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Act.Core.IntegrationTests.Controllers
{
	public class WebHookControllerTests : IClassFixture<WebAppFactory>
	{
		private readonly WebAppFactory _factory;
		public WebHookControllerTests(WebAppFactory factory) => _factory = factory;

		[Fact]
		public async Task Data_Should_Return_Created_On_Valid_Payload()
		{
			var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
			var json = "{\"type\":\"converge_failure\",\"automate_fqdn\":\"chef.example.com\",\"automate_failure_url\":\"https://chef.example.com/infrastructure/client-runs/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/runs/xxx\"}";
			var response = await client.PostAsync("/WebHook/Data", new StringContent(json, Encoding.UTF8, "application/json"));
			response.StatusCode.Should().Be(HttpStatusCode.Created);
		}
	}
}