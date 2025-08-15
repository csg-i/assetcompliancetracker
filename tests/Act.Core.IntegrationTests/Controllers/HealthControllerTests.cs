using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Act.Core.IntegrationTests.Controllers
{
	public class HealthControllerTests : IClassFixture<WebAppFactory>
	{
		private readonly WebAppFactory _factory;
		public HealthControllerTests(WebAppFactory factory)
		{
			_factory = factory;
		}

		[Fact]
		public async Task Index_Should_Render_Simple_Health_View_Without_Auth()
		{
			var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
			{
				AllowAutoRedirect = false
			});
			var response = await client.GetAsync("/Health");
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var html = await response.Content.ReadAsStringAsync();
			html.Should().Contain("Simple Health Check");
			html.Should().Contain("Database Connected");
		}

		[Theory]
		[InlineData("404", "404")]
		[InlineData("500", "500")]
		public async Task Error_Should_Render_Error_Page(string code, string expected)
		{
			var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
			{
				AllowAutoRedirect = false
			});
			var response = await client.GetAsync($"/Health/Error/{code}");
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var html = await response.Content.ReadAsStringAsync();
			html.Should().Contain(expected);
		}
	}
}