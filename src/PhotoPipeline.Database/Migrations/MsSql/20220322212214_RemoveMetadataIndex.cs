using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoPipeline.Database.Migrations.mssql
{
    public partial class RemoveMetadataIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PhotoMetadata_Key_Value",
                table: "PhotoMetadata");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "PhotoMetadata",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "PhotoMetadata",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoMetadata_Key_Value",
                table: "PhotoMetadata",
                columns: new[] { "Key", "Value" });
        }
    }
}
