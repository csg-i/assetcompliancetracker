using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace act.core.data.Migrations
{
    public partial class UpdateNodeWithRemedyGroupInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "SoftwareComponent",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(DateTime))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "Port",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(DateTime))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<string>(
                name: "RemedyGroupEmailList",
                table: "Node",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemedyGroupName",
                table: "Node",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "Justification",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(DateTime))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "BuildSpecification",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(DateTime))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemedyGroupEmailList",
                table: "Node");

            migrationBuilder.DropColumn(
                name: "RemedyGroupName",
                table: "Node");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "SoftwareComponent",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldRowVersion: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "Port",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldRowVersion: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "Justification",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldRowVersion: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "BuildSpecification",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldRowVersion: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);
        }
    }
}
