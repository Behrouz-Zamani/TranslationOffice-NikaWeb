using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranslationOffice.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderDeliveryFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderDeliveryFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    TranslatorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDeliveryFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDeliveryFiles_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderDeliveryFiles_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDeliveryFiles_OrderId",
                table: "OrderDeliveryFiles",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDeliveryFiles_OrderItemId",
                table: "OrderDeliveryFiles",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDeliveryFiles_TranslatorId",
                table: "OrderDeliveryFiles",
                column: "TranslatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDeliveryFiles");
        }
    }
}
