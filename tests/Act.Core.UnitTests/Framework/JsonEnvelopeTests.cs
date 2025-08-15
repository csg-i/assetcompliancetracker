using System.Collections.Generic;
using act.core.web.Framework;
using FluentAssertions;
using Xunit;

namespace Act.Core.UnitTests.Framework
{
	public class JsonEnvelopeTests
	{
		[Fact]
		public void Success_NoData_Should_Set_Status_And_Default_Data()
		{
			var env = JsonEnvelope.Success();
			env.status.Should().Be("success");
			env.data.Should().NotBeNull();
		}

		[Fact]
		public void Success_WithData_Should_Set_Data()
		{
			var env = JsonEnvelope.Success(new { a = 1 });
			env.status.Should().Be("success");
			env.data.Should().BeEquivalentTo(new { a = 1 });
		}

		[Fact]
		public void Error_Should_Accept_Message_And_FieldPairs()
		{
			JsonEnvelope.Error("bad").status.Should().Be("error");
			JsonEnvelope.Error(new List<KeyValuePair<string, string>> { new("Name", "Required") }).status.Should().Be("error");
		}
	}
}