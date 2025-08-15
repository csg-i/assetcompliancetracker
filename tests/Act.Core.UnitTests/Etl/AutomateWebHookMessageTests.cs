using System;
using act.core.etl.ComplianceModel;
using FluentAssertions;
using Xunit;

namespace Act.Core.UnitTests.Etl
{
	public class AutomateWebHookMessageTests
	{
		[Fact]
		public void ChefNodeId_Should_Parse_From_Url_When_ConvergeFailure()
		{
			var guid = Guid.NewGuid();
			var msg = new AutomateWebHookMessage
			{
				type = "converge_failure",
				automate_fqdn = "chef.example.com",
				automate_failure_url = $"https://chef.example.com/infrastructure/client-runs/{guid}/runs/abc"
			};
			msg.ChefNodeId.Should().Be(guid);
		}

		[Fact]
		public void ChefNodeId_Should_Return_NodeUuid_When_ComplianceFailure()
		{
			var guid = Guid.NewGuid();
			var msg = new AutomateWebHookMessage { type = "compliance_failure", node_uuid = guid };
			msg.ChefNodeId.Should().Be(guid);
		}

		[Fact]
		public void IsComplianceFailure_And_IsNodeFailure_Should_Work()
		{
			new AutomateWebHookMessage { type = "compliance_failure" }.IsComplianceFailure.Should().BeTrue();
			new AutomateWebHookMessage { type = "converge_failure" }.IsNodeFailure.Should().BeTrue();
		}
	}
}