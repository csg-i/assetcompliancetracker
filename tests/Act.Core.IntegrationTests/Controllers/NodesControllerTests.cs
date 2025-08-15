using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Act.Core.IntegrationTests.Controllers
{
	public class NodesControllerTests : IClassFixture<WebAppFactory>
	{
		private readonly WebAppFactory _factory;
		public NodesControllerTests(WebAppFactory factory) => _factory = factory;

		[Fact]
		public async Task BuildSpecId_Should_Return_Success_Envelope()
		{
			var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
			var response = await client.PostAsync("/Nodes/BuildSpecId", new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("host", "foo") }));
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var json = await response.Content.ReadAsStringAsync();
			json.Should().Contain("success");
		}
	}
}