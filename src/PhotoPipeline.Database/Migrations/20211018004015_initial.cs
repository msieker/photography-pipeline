using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace PhotoPipeline.Database.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "photos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_file_name = table.Column<string>(type: "text", nullable: false),
                    intake_take = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    hash = table.Column<string>(type: "text", nullable: false),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    taken = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    location = table.Column<Point>(type: "geometry", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_photos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "photo_hashes",
                columns: table => new
                {
                    photo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hash_type = table.Column<string>(type: "text", nullable: false),
                    hash_value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_photo_hashes", x => new { x.photo_id, x.hash_type });
                    table.ForeignKey(
                        name: "fk_photo_hashes_photos_photo_id",
                        column: x => x.photo_id,
                        principalTable: "photos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "photo_metadata",
                columns: table => new
                {
                    photo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_photo_metadata", x => new { x.photo_id, x.key });
                    table.ForeignKey(
                        name: "fk_photo_metadata_photos_photo_id",
                        column: x => x.photo_id,
                        principalTable: "photos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_photo_hashes_hash_type_hash_value",
                table: "photo_hashes",
                columns: new[] { "hash_type", "hash_value" });

            migrationBuilder.CreateIndex(
                name: "ix_photo_metadata_key_value",
                table: "photo_metadata",
                columns: new[] { "key", "value" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "photo_hashes");

            migrationBuilder.DropTable(
                name: "photo_metadata");

            migrationBuilder.DropTable(
                name: "photos");
        }
    }
}
