using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoPipeline.Database.Migrations.mssql
{
    public partial class AddNewFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "PhotoMetadata",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "PhotoHashes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "PhotoMetadata");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "PhotoHashes");
        }
    }
}
