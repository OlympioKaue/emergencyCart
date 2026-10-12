using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmergencyCart.API.Migrations
{
    /// <inheritdoc />
    public partial class v11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "Inspection",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Inspection_EmployeeId",
                table: "Inspection",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inspection_Employee_EmployeeId",
                table: "Inspection",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inspection_Employee_EmployeeId",
                table: "Inspection");

            migrationBuilder.DropIndex(
                name: "IX_Inspection_EmployeeId",
                table: "Inspection");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Inspection");
        }
    }
}
