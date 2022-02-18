using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PhotoPipeline.Database.Migrations
{
    public partial class AddRunPipelineSteps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "run_pipeline_step",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    photo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_name = table.Column<string>(type: "text", nullable: false),
                    step_version = table.Column<int>(type: "integer", nullable: false),
                    processed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_run_pipeline_step", x => x.id);
                    table.ForeignKey(
                        name: "fk_run_pipeline_step_photos_photo_id",
                        column: x => x.photo_id,
                        principalTable: "photos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_run_pipeline_step_photo_id",
                table: "run_pipeline_step",
                column: "photo_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "run_pipeline_step");
        }
    }
}
