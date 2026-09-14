using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmergencyCart.API.Migrations
{
    /// <inheritdoc />
    public partial class v8AlterTableEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "VARCHAR(20)", maxLength: 20, nullable: false),
                    LastName = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "VARCHAR(70)", maxLength: 70, nullable: false),
                    Hash = table.Column<string>(type: "VARCHAR(70)", maxLength: 70, nullable: false),
                    Code = table.Column<string>(type: "VARCHAR(30)", maxLength: 30, nullable: false),
                    Role = table.Column<int>(type: "INT", nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "I_Employee_Email_Address",
                table: "Employee",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false),
                    Role = table.Column<int>(type: "INT", nullable: false),
                    Code = table.Column<string>(type: "VARCHAR(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "VARCHAR(70)", maxLength: 70, nullable: false),
                    FirstName = table.Column<string>(type: "VARCHAR(20)", maxLength: 20, nullable: false),
                    LastName = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false),
                    Hash = table.Column<string>(type: "VARCHAR(70)", maxLength: 70, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "I_User_Email_Address",
                table: "User",
                column: "Email",
                unique: true);
        }
    }
}
