using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace act.core.data.Migrations
{
    [DbContext(typeof(ActDbContext))]
    [Migration("20260923103000_AddNodeEscalationMail")]
    public partial class AddNodeEscalationMail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EscalationMailedOn",
                table: "Node",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EscalationStep",
                table: "Node",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EscalationMailedOn",
                table: "Node");

            migrationBuilder.DropColumn(
                name: "EscalationStep",
                table: "Node");
        }
    }
}
