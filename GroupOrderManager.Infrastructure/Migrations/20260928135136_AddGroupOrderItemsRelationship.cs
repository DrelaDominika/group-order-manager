using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupOrderManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupOrderItemsRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GroupOrderItems_GroupOrderId",
                table: "GroupOrderItems",
                column: "GroupOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupOrderItems_GroupOrders_GroupOrderId",
                table: "GroupOrderItems",
                column: "GroupOrderId",
                principalTable: "GroupOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupOrderItems_GroupOrders_GroupOrderId",
                table: "GroupOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_GroupOrderItems_GroupOrderId",
                table: "GroupOrderItems");
        }
    }
}
