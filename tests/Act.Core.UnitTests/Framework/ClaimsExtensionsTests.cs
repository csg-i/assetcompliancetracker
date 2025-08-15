using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;
using act.core.web.Framework;

namespace Act.Core.UnitTests.Framework
{
	public class ClaimsExtensionsTests
	{
		[Fact]
		public void IsAjaxRequest_Should_Read_Header_Or_Query()
		{
			var context = new DefaultHttpContext();
			context.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
			context.Request.IsAjaxRequest().Should().BeTrue();

			var context2 = new DefaultHttpContext();
			context2.Request.QueryString = new QueryString("?X-Requested-With=XMLHttpRequest");
			context2.Request.IsAjaxRequest().Should().BeTrue();

			var context3 = new DefaultHttpContext();
			context3.Request.IsAjaxRequest().Should().BeFalse();
		}

		[Fact]
		public void IsAjaxHtml_Should_Read_Header_Or_Query()
		{
			const string accept = "text/html, */*; q=0.01";
			var context = new DefaultHttpContext();
			context.Request.Headers["Accept"] = accept;
			context.Request.IsAjaxHtml().Should().BeTrue();

			var context2 = new DefaultHttpContext();
			context2.Request.QueryString = new QueryString($"?Accept={accept}");
			context2.Request.IsAjaxHtml().Should().BeTrue();

			var context3 = new DefaultHttpContext();
			context3.Request.IsAjaxHtml().Should().BeFalse();
		}

		[Fact]
		public void GetClaim_Should_Return_First_Match_And_Null_When_Absent()
		{
			var claims = new List<Claim>
			{
				new Claim("type1", "value1"),
				new Claim("type1", "value2"),
				new Claim("type2", "value3")
			};
			claims.GetClaim("type1").Should().Be("value1");
			claims.GetClaim("missing").Should().BeNull();
		}

		[Fact]
		public void GetClaimAsInt64_Should_Parse_Or_Return_Zero()
		{
			var claims = new List<Claim> { new Claim("id", "123"), new Claim("bad", "x") };
			claims.GetClaimAsInt64("id").Should().Be(123);
			claims.GetClaimAsInt64("bad").Should().Be(0);
			claims.GetClaimAsInt64("missing").Should().Be(0);
		}
	}
}