using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TranslationOffice.Data;

#nullable disable

namespace TranslationOffice.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261112160000_AddDocumentSampleImage")]
public partial class AddDocumentSampleImage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SampleImagePath",
            table: "DocumentTypes",
            type: "nvarchar(300)",
            maxLength: 300,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SampleImagePath",
            table: "DocumentTypes");
    }
}
