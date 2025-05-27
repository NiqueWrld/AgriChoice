using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriChoice.Migrations
{
    /// <inheritdoc />
    public partial class initial33 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_RefundRequests_RefundRequestId1",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_RefundRequestId1",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "RefundRequestId1",
                table: "Purchases");

            migrationBuilder.AlterColumn<string>(
                name: "DriverId",
                table: "RefundRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "RefundRequestCow",
                columns: table => new
                {
                    RefundRequestId = table.Column<int>(type: "int", nullable: false),
                    CowId = table.Column<int>(type: "int", nullable: false),
                    ReturnReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Condition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Processed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRequestCow", x => new { x.RefundRequestId, x.CowId });
                    table.ForeignKey(
                        name: "FK_RefundRequestCow_Cows_CowId",
                        column: x => x.CowId,
                        principalTable: "Cows",
                        principalColumn: "CowId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RefundRequestCow_RefundRequests_RefundRequestId",
                        column: x => x.RefundRequestId,
                        principalTable: "RefundRequests",
                        principalColumn: "RefundRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_DriverId",
                table: "RefundRequests",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_PurchaseId",
                table: "RefundRequests",
                column: "PurchaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequestCow_CowId",
                table: "RefundRequestCow",
                column: "CowId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefundRequests_AspNetUsers_DriverId",
                table: "RefundRequests",
                column: "DriverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefundRequests_Purchases_PurchaseId",
                table: "RefundRequests",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "PurchaseId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefundRequests_AspNetUsers_DriverId",
                table: "RefundRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundRequests_Purchases_PurchaseId",
                table: "RefundRequests");

            migrationBuilder.DropTable(
                name: "RefundRequestCow");

            migrationBuilder.DropIndex(
                name: "IX_RefundRequests_DriverId",
                table: "RefundRequests");

            migrationBuilder.DropIndex(
                name: "IX_RefundRequests_PurchaseId",
                table: "RefundRequests");

            migrationBuilder.AlterColumn<string>(
                name: "DriverId",
                table: "RefundRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RefundRequestId1",
                table: "Purchases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_RefundRequestId1",
                table: "Purchases",
                column: "RefundRequestId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_RefundRequests_RefundRequestId1",
                table: "Purchases",
                column: "RefundRequestId1",
                principalTable: "RefundRequests",
                principalColumn: "RefundRequestId");
        }
    }
}
