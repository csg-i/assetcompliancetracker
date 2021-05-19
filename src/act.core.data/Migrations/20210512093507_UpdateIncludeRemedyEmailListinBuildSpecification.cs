using Microsoft.EntityFrameworkCore.Migrations;

namespace act.core.data.Migrations
{
    public partial class UpdateIncludeRemedyEmailListinBuildSpecification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IncludeRemedyEmailList",
                table: "BuildSpecification",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludeRemedyEmailList",
                table: "BuildSpecification");
        }
    }
}
