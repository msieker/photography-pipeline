using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoPipeline.Database.Migrations.mssql
{
    public partial class AdjustKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RunPipelineStep");

            migrationBuilder.CreateTable(
                name: "PhotoPipelineStep",
                columns: table => new
                {
                    PhotoId = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StepVersion = table.Column<int>(type: "int", nullable: false),
                    Processed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoPipelineStep", x => new { x.PhotoId, x.StepName });
                    table.ForeignKey(
                        name: "FK_PhotoPipelineStep_Photos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhotoPipelineStep");

            migrationBuilder.CreateTable(
                name: "RunPipelineStep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhotoId = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    Processed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StepVersion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunPipelineStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RunPipelineStep_Photos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RunPipelineStep_PhotoId",
                table: "RunPipelineStep",
                column: "PhotoId");
        }
    }
}
