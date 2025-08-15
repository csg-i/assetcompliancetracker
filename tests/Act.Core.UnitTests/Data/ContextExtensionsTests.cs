using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory;
using Moq;
using Xunit;

namespace Act.Core.UnitTests.Data
{
	public class ContextExtensionsTests
	{
		[Flags]
		private enum Demo : int { A = 1, B = 2, C = 4 }

		[Fact]
		public void ConvertToFlag_Should_Combine_Enum_Flags_And_Handle_Empty()
		{
			new Demo[] { Demo.A, Demo.C }.ConvertToFlag().Should().Be(Demo.A | Demo.C);
			Array.Empty<Demo>().ConvertToFlag().Should().BeNull();
		}

		[Fact]
		public void OwnerText_Should_Build_Display_Name_With_Or_Without_Sam()
		{
			var e = new Employee { FirstName = "John", LastName = "Doe", SamAccountName = "jdoe" };
			e.OwnerText().Should().Be("John Doe (jdoe)");
			e.PreferredName = "Johnny";
			e.OwnerText(false).Should().Be("Johnny");
			((Employee)null).OwnerText().Should().Be(string.Empty);
		}

		[Fact]
		public void GetEnvironmentNames_Should_Map_Ids_To_Names_And_Join()
		{
			var scs = new[]
			{
				new SoftwareComponentEnvironment { EnvironmentId = 12 },
				new SoftwareComponentEnvironment { EnvironmentId = 1 },
				new SoftwareComponentEnvironment { EnvironmentId = 19 },
			};
			scs.GetEnvironmentNames().Should().Be("CTE/NonProd/Production");
		}

		[Fact]
		public void Queryable_Filter_Extensions_Should_Compose_Predicates()
		{
			var options = new DbContextOptionsBuilder<ActDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
				.Options;

			using var ctx = new ActDbContext(options);
			Seed(ctx);

			ctx.Nodes.AsQueryable().Active().Count().Should().Be(2);
			ctx.Nodes.AsQueryable().Inactive().Count().Should().Be(1);
			ctx.Nodes.AsQueryable().InPciScope().Count().Should().Be(2);
			ctx.Nodes.AsQueryable().ByPciScope(PciScopeConstant.A).Count().Should().Be(1);
			ctx.Nodes.AsQueryable().ByPlatforms(new[] { PlatformConstant.Linux, PlatformConstant.WindowsServer }).Count().Should().Be(2);
			ctx.Nodes.AsQueryable().ProductIsNotExlcuded().Count().Should().Be(2);
			ctx.Nodes.AsQueryable().ProductIsExlcuded().Count().Should().Be(1);
		}

		private static void Seed(ActDbContext ctx)
		{
			ctx.Products.AddRange(new Product { Code = "A001", Name = "ProdA", ExludeFromReports = false }, new Product { Code = "B001", Name = "ProdB", ExludeFromReports = true });
			ctx.Employees.Add(new Employee { Id = 1, FirstName = "X", LastName = "Y", SamAccountName = "xy" });
			ctx.Environments.Add(new Environment { Id = 1, Name = "NonProd", Description = "np", ChefAutomateUrl = "https://a", ChefAutomateOrg = "o", ChefAutomateToken = "t", Color = "#fff" });
			ctx.Nodes.AddRange(
				new Node { InventoryItemId = 1, Fqdn = "a", OwnerEmployeeId = 1, ProductCode = "A001", EnvironmentId = 1, PciScope = PciScopeConstant.A, Platform = PlatformConstant.Linux, IsActive = true },
				new Node { InventoryItemId = 2, Fqdn = "b", OwnerEmployeeId = 1, ProductCode = "A001", EnvironmentId = 1, PciScope = PciScopeConstant.B, Platform = PlatformConstant.WindowsServer, IsActive = true },
				new Node { InventoryItemId = 3, Fqdn = "c", OwnerEmployeeId = 1, ProductCode = "B001", EnvironmentId = 1, PciScope = PciScopeConstant.C, Platform = PlatformConstant.Other, IsActive = false }
			);
			ctx.SaveChanges();
		}
	}
}