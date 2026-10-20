using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace act.core.data.Migrations
{
    [DbContext(typeof(ActDbContext))]
    [Migration("20261020120000_AddEmployeePciDirectorId")]
    public partial class AddEmployeePciDirectorId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PciDirectorId",
                table: "Employee",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_PciDirectorId",
                table: "Employee",
                column: "PciDirectorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employee_PciDirectorId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "PciDirectorId",
                table: "Employee");
        }
    }
}
