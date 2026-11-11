using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranslationOffice.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestOrderToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuestAccessTokenHash",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_GuestAccessTokenHash",
                table: "Orders",
                column: "GuestAccessTokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_GuestAccessTokenHash",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GuestAccessTokenHash",
                table: "Orders");
        }
    }
}
