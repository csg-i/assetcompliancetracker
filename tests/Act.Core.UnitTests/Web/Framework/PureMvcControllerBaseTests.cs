using System;
using System.Threading.Tasks;
using act.core.web.Framework;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Act.Core.UnitTests.Web.Framework
{
	public class PureMvcControllerBaseTests
	{
		private class FakeController : PureMvcControllerBase
		{
			public FakeController(ILoggerFactory loggerFactory) : base(loggerFactory) { }
			public IActionResult Echo() => new OkResult();
		}

		[Fact]
		public void GetUri_Should_Compose_From_Request()
		{
			var loggerFactory = LoggerFactory.Create(b => { });
			var controller = new FakeController(loggerFactory);
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Scheme = "https";
			httpContext.Request.Host = new HostString("localhost", 5001);
			httpContext.Request.Path = "/Nodes/Automate";
			httpContext.Request.QueryString = new QueryString("?a=1&b=2");
			controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

			var uri = controller.GetUri();
			uri.Should().Be(new Uri("https://localhost:5001/Nodes/Automate?a=1&b=2"));
		}
	}
}