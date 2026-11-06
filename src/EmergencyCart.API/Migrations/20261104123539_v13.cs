using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmergencyCart.API.Migrations
{
    /// <inheritdoc />
    public partial class v13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Replenishment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReplenishedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Replenishment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Replenishment_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Replenishment_InspectionItem_InspectionItemId",
                        column: x => x.InspectionItemId,
                        principalTable: "InspectionItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Replenishment_EmployeeId",
                table: "Replenishment",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Replenishment_InspectionItemId",
                table: "Replenishment",
                column: "InspectionItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Replenishment");
        }
    }
}
