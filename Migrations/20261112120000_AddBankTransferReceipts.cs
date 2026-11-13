using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TranslationOffice.Data;

#nullable disable

namespace TranslationOffice.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261112120000_AddBankTransferReceipts")]
public partial class AddBankTransferReceipts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PayerName",
            table: "Payments",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReceiptContentType",
            table: "Payments",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "ReceiptFileSize",
            table: "Payments",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReceiptOriginalFileName",
            table: "Payments",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReceiptStoredFileName",
            table: "Payments",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "ReviewedAt",
            table: "Payments",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReviewedByUserId",
            table: "Payments",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TransferReference",
            table: "Payments",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PayerName", table: "Payments");
        migrationBuilder.DropColumn(name: "ReceiptContentType", table: "Payments");
        migrationBuilder.DropColumn(name: "ReceiptFileSize", table: "Payments");
        migrationBuilder.DropColumn(name: "ReceiptOriginalFileName", table: "Payments");
        migrationBuilder.DropColumn(name: "ReceiptStoredFileName", table: "Payments");
        migrationBuilder.DropColumn(name: "ReviewedAt", table: "Payments");
        migrationBuilder.DropColumn(name: "ReviewedByUserId", table: "Payments");
        migrationBuilder.DropColumn(name: "TransferReference", table: "Payments");
    }
}
