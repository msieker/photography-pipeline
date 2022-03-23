using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoPipeline.Database.Migrations.mssql
{
    public partial class AddRemovedFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Removed",
                table: "Photos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Removed",
                table: "Photos");
        }
    }
}
