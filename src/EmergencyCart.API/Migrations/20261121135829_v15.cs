using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmergencyCart.API.Migrations
{
    /// <inheritdoc />
    public partial class v15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExportLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Format = table.Column<int>(type: "int", nullable: false),
                    EmergencyCartId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecordCount = table.Column<int>(type: "int", nullable: false),
                    ExportedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExportLog_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inspection_EmergencyCartId_Status_FinishedAt",
                table: "Inspection",
                columns: new[] { "EmergencyCartId", "Status", "FinishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Inspection_Status_FinishedAt",
                table: "Inspection",
                columns: new[] { "Status", "FinishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ExportLog_EmployeeId",
                table: "ExportLog",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportLog_ExportedAt",
                table: "ExportLog",
                column: "ExportedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExportLog");

            migrationBuilder.DropIndex(
                name: "IX_Inspection_EmergencyCartId_Status_FinishedAt",
                table: "Inspection");

            migrationBuilder.DropIndex(
                name: "IX_Inspection_Status_FinishedAt",
                table: "Inspection");
        }
    }
}
