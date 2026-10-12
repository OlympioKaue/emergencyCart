using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmergencyCart.API.Migrations
{
    /// <inheritdoc />
    public partial class v10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InspectionItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpectedQuantity = table.Column<int>(type: "int", nullable: false),
                    FoundQuantity = table.Column<int>(type: "int", nullable: true),
                    InspectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionItem_Inspection_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InspectionItem_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionItem_InspectionId_ItemId",
                table: "InspectionItem",
                columns: new[] { "InspectionId", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionItem_ItemId",
                table: "InspectionItem",
                column: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionItem");
        }
    }
}
