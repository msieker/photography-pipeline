using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace PhotoPipeline.Database.Migrations.mssql
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    OriginalPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntakeTake = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StoredPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    Taken = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<Point>(type: "geography", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhotoHashes",
                columns: table => new
                {
                    PhotoId = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    HashType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HashValue = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoHashes", x => new { x.PhotoId, x.HashType });
                    table.ForeignKey(
                        name: "FK_PhotoHashes_Photos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhotoMetadata",
                columns: table => new
                {
                    PhotoId = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoMetadata", x => new { x.PhotoId, x.Key });
                    table.ForeignKey(
                        name: "FK_PhotoMetadata_Photos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RunPipelineStep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhotoId = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StepVersion = table.Column<int>(type: "int", nullable: false),
                    Processed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
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
                name: "IX_PhotoHashes_HashType_HashValue",
                table: "PhotoHashes",
                columns: new[] { "HashType", "HashValue" });

            migrationBuilder.CreateIndex(
                name: "IX_PhotoMetadata_Key_Value",
                table: "PhotoMetadata",
                columns: new[] { "Key", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_RunPipelineStep_PhotoId",
                table: "RunPipelineStep",
                column: "PhotoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhotoHashes");

            migrationBuilder.DropTable(
                name: "PhotoMetadata");

            migrationBuilder.DropTable(
                name: "RunPipelineStep");

            migrationBuilder.DropTable(
                name: "Photos");
        }
    }
}
