using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmergencyCart.API.Migrations
{
    /// <inheritdoc />
    public partial class v6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CartItem_EmergencyCartId",
                table: "CartItem");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "EmergencyCart",
                type: "VARCHAR(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(25)",
                oldMaxLength: 25);

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_EmergencyCartId_ItemId",
                table: "CartItem",
                columns: new[] { "EmergencyCartId", "ItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CartItem_EmergencyCartId_ItemId",
                table: "CartItem");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "EmergencyCart",
                type: "VARCHAR(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(60)",
                oldMaxLength: 60);

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_EmergencyCartId",
                table: "CartItem",
                column: "EmergencyCartId");
        }
    }
}
