using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TranslationOffice.Data;

#nullable disable

namespace TranslationOffice.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261112190000_AddShoppingCarts")]
public partial class AddShoppingCarts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ShoppingCarts",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CartKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CartJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                OrderId = table.Column<int>(type: "int", nullable: true),
                ItemCount = table.Column<int>(type: "int", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ShoppingCarts", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_ShoppingCarts_CartKey",
            table: "ShoppingCarts",
            column: "CartKey",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ShoppingCarts");
    }
}
