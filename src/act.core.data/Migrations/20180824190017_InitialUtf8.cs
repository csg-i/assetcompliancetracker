using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace act.core.data.Migrations
{
    public partial class InitialUtf8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {   
            migrationBuilder.Sql($@"ALTER DATABASE CHARACTER SET utf8 COLLATE utf8_general_ci;");
            migrationBuilder.Sql($@"ALTER DATABASE DEFAULT CHARACTER SET utf8 DEFAULT COLLATE utf8_general_ci;");
            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    FirstName = table.Column<string>(maxLength: 32, nullable: false),
                    LastName = table.Column<string>(maxLength: 32, nullable: false),
                    SamAccountName = table.Column<string>(type: "nchar(64)", nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    PreferredName = table.Column<string>(maxLength: 64, nullable: true),
                    SupervisorId = table.Column<long>(nullable: true),
                    ReportingDirectorId = table.Column<long>(nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employee_Employee_ReportingDirectorId",
                        column: x => x.ReportingDirectorId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employee_Employee_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Environment",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 32, nullable: false),
                    Description = table.Column<string>(maxLength: 512, nullable: false),
                    ChefAutomateUrl = table.Column<string>(maxLength: 256, nullable: false),
                    ChefAutomateToken = table.Column<string>(maxLength: 64, nullable: false),
                    ChefAutomateOrg = table.Column<string>(maxLength: 16, nullable: false),
                    Color = table.Column<string>(maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Environment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Function",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Function", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nchar(4)", nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    ExludeFromReports = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "BuildSpecification",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BuildSpecificationType = table.Column<int>(nullable: false),
                    ParentId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    OwnerEmployeeId = table.Column<long>(nullable: false),
                    Platform = table.Column<int>(nullable: true),
                    OperatingSystemName = table.Column<string>(maxLength: 256, nullable: true),
                    OperatingSystemVersion = table.Column<string>(maxLength: 32, nullable: true),
                    WikiLink = table.Column<string>(maxLength: 256, nullable: true),
                    Overview = table.Column<string>(nullable: true),
                    RunningCoreOs = table.Column<bool>(nullable: false),
                    EmailAddress = table.Column<string>(maxLength: 256, nullable: true),
                    TimeStamp = table.Column<DateTime>(rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildSpecification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildSpecification_Employee_OwnerEmployeeId",
                        column: x => x.OwnerEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuildSpecification_BuildSpecification_ParentId",
                        column: x => x.ParentId,
                        principalTable: "BuildSpecification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Justification",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BuildSpecificationId = table.Column<long>(nullable: false),
                    JustificationType = table.Column<int>(nullable: false),
                    JustificationText = table.Column<string>(nullable: false),
                    TimeStamp = table.Column<DateTime>(rowVersion: true, nullable: false),
                    Color = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Justification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Justification_BuildSpecification_BuildSpecificationId",
                        column: x => x.BuildSpecificationId,
                        principalTable: "BuildSpecification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Node",
                columns: table => new
                {
                    InventoryItemId = table.Column<long>(nullable: false),
                    Fqdn = table.Column<string>(maxLength: 256, nullable: true),
                    OwnerEmployeeId = table.Column<long>(nullable: false),
                    ProductCode = table.Column<string>(type: "nchar(4)", nullable: true),
                    FunctionId = table.Column<int>(nullable: false),
                    PciScope = table.Column<int>(nullable: false, defaultValueSql: "4"),
                    EnvironmentId = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    DeactivatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    BuildSpecificationId = table.Column<long>(nullable: true),
                    Platform = table.Column<int>(nullable: false),
                    ComplianceStatus = table.Column<int>(nullable: false),
                    LastComplianceResultDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    FailingSince = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastEmailedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    ChefNodeId = table.Column<Guid>(nullable: true),
                    LastComplianceResultId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Node", x => x.InventoryItemId);
                    table.ForeignKey(
                        name: "FK_Node_BuildSpecification_BuildSpecificationId",
                        column: x => x.BuildSpecificationId,
                        principalTable: "BuildSpecification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Node_Environment_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "Environment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Node_Function_FunctionId",
                        column: x => x.FunctionId,
                        principalTable: "Function",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Node_Employee_OwnerEmployeeId",
                        column: x => x.OwnerEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Node_Product_ProductCode",
                        column: x => x.ProductCode,
                        principalTable: "Product",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Port",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BuildSpecificationId = table.Column<long>(nullable: false),
                    PortType = table.Column<int>(nullable: false),
                    From = table.Column<int>(nullable: false),
                    To = table.Column<int>(nullable: true),
                    TimeStamp = table.Column<DateTime>(rowVersion: true, nullable: false),
                    IsExternal = table.Column<bool>(nullable: false),
                    IsOutgoing = table.Column<bool>(nullable: false),
                    JustificationId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Port", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Port_BuildSpecification_BuildSpecificationId",
                        column: x => x.BuildSpecificationId,
                        principalTable: "BuildSpecification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Port_Justification_JustificationId",
                        column: x => x.JustificationId,
                        principalTable: "Justification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareComponent",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BuildSpecificationId = table.Column<long>(nullable: false),
                    JustificationType = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    NonCore = table.Column<bool>(nullable: false),
                    TimeStamp = table.Column<DateTime>(rowVersion: true, nullable: false),
                    JustificationId = table.Column<long>(nullable: true),
                    PciScope = table.Column<int>(nullable: true),
                    Description = table.Column<string>(maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareComponent_BuildSpecification_BuildSpecificationId",
                        column: x => x.BuildSpecificationId,
                        principalTable: "BuildSpecification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareComponent_Justification_JustificationId",
                        column: x => x.JustificationId,
                        principalTable: "Justification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceResult",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ResultId = table.Column<Guid>(nullable: false),
                    InventoryItemId = table.Column<long>(nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<int>(nullable: false),
                    OperatingSystemTestPassed = table.Column<bool>(nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceResult_Node_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "Node",
                        principalColumn: "InventoryItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareComponentEnvironment",
                columns: table => new
                {
                    EnvironmentId = table.Column<int>(nullable: false),
                    SoftwareComponentId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareComponentEnvironment", x => new { x.EnvironmentId, x.SoftwareComponentId });
                    table.ForeignKey(
                        name: "FK_SoftwareComponentEnvironment_Environment_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "Environment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareComponentEnvironment_SoftwareComponent_SoftwareCompo~",
                        column: x => x.SoftwareComponentId,
                        principalTable: "SoftwareComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceResultError",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ComplianceResultId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    Code = table.Column<string>(maxLength: 256, nullable: false),
                    LongMessage = table.Column<string>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceResultError", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceResultError_ComplianceResult_ComplianceResultId",
                        column: x => x.ComplianceResultId,
                        principalTable: "ComplianceResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceResultTest",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ComplianceResultId = table.Column<long>(nullable: false),
                    ResultType = table.Column<int>(nullable: false),
                    ShouldExist = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    Status = table.Column<int>(nullable: false),
                    PortType = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceResultTest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceResultTest_ComplianceResult_ComplianceResultId",
                        column: x => x.ComplianceResultId,
                        principalTable: "ComplianceResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildSpecification_Name",
                table: "BuildSpecification",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildSpecification_OwnerEmployeeId",
                table: "BuildSpecification",
                column: "OwnerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildSpecification_ParentId",
                table: "BuildSpecification",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceResult_InventoryItemId",
                table: "ComplianceResult",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceResult_ResultId_InventoryItemId",
                table: "ComplianceResult",
                columns: new[] { "ResultId", "InventoryItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceResult_EndDate_Id_InventoryItemId",
                table: "ComplianceResult",
                columns: new[] { "EndDate", "Id", "InventoryItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceResultError_ComplianceResultId_Code_Name",
                table: "ComplianceResultError",
                columns: new[] { "ComplianceResultId", "Code", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceResultTest_ComplianceResultId_ResultType_PortType_~",
                table: "ComplianceResultTest",
                columns: new[] { "ComplianceResultId", "ResultType", "PortType", "ShouldExist", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_ReportingDirectorId",
                table: "Employee",
                column: "ReportingDirectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_SamAccountName",
                table: "Employee",
                column: "SamAccountName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_SupervisorId",
                table: "Employee",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_FirstName_Id",
                table: "Employee",
                columns: new[] { "FirstName", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_LastName_Id",
                table: "Employee",
                columns: new[] { "LastName", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_PreferredName_Id",
                table: "Employee",
                columns: new[] { "PreferredName", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Environment_Name",
                table: "Environment",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Function_Name",
                table: "Function",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Justification_BuildSpecificationId",
                table: "Justification",
                column: "BuildSpecificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Node_BuildSpecificationId",
                table: "Node",
                column: "BuildSpecificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Node_EnvironmentId",
                table: "Node",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Node_FunctionId",
                table: "Node",
                column: "FunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_Node_OwnerEmployeeId",
                table: "Node",
                column: "OwnerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Node_Platform",
                table: "Node",
                column: "Platform");

            migrationBuilder.CreateIndex(
                name: "IX_Node_ProductCode",
                table: "Node",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_Node_Fqdn_InventoryItemId",
                table: "Node",
                columns: new[] { "Fqdn", "InventoryItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_Node_PciScope_InventoryItemId",
                table: "Node",
                columns: new[] { "PciScope", "InventoryItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_Port_BuildSpecificationId",
                table: "Port",
                column: "BuildSpecificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Port_JustificationId",
                table: "Port",
                column: "JustificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Name",
                table: "Product",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponent_BuildSpecificationId",
                table: "SoftwareComponent",
                column: "BuildSpecificationId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponent_JustificationId",
                table: "SoftwareComponent",
                column: "JustificationId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponentEnvironment_SoftwareComponentId",
                table: "SoftwareComponentEnvironment",
                column: "SoftwareComponentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplianceResultError");

            migrationBuilder.DropTable(
                name: "ComplianceResultTest");

            migrationBuilder.DropTable(
                name: "Port");

            migrationBuilder.DropTable(
                name: "SoftwareComponentEnvironment");

            migrationBuilder.DropTable(
                name: "ComplianceResult");

            migrationBuilder.DropTable(
                name: "SoftwareComponent");

            migrationBuilder.DropTable(
                name: "Node");

            migrationBuilder.DropTable(
                name: "Justification");

            migrationBuilder.DropTable(
                name: "Environment");

            migrationBuilder.DropTable(
                name: "Function");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "BuildSpecification");

            migrationBuilder.DropTable(
                name: "Employee");
        }
    }
}
