using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TranslationOffice.Data;

#nullable disable

namespace TranslationOffice.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261112143000_AddDocumentDescriptions")]
public partial class AddDocumentDescriptions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "DocumentTypes",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Description",
            table: "DocumentTypes");
    }
}