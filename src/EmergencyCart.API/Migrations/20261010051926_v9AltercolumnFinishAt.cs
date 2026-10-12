using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmergencyCart.API.Migrations
{
    /// <inheritdoc />
    public partial class v9AltercolumnFinishAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inspection_EmergencyCartId",
                table: "Inspection");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAt",
                table: "Inspection",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inspection_EmergencyCartId",
                table: "Inspection",
                column: "EmergencyCartId",
                unique: true,
                filter: "[Status] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inspection_EmergencyCartId",
                table: "Inspection");

            migrationBuilder.DropColumn(
                name: "FinishedAt",
                table: "Inspection");

            migrationBuilder.CreateIndex(
                name: "IX_Inspection_EmergencyCartId",
                table: "Inspection",
                column: "EmergencyCartId");
        }
    }
}
