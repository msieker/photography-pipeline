﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PhotoPipeline.Database;

#nullable disable

namespace PhotoPipeline.Database.Migrations
{
    [DbContext(typeof(PhotoDbContext))]
    [Migration("20211018011245_AddRunPipelineSteps")]
    partial class AddRunPipelineSteps
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0-rtm.21477.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "postgis");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PhotoPipeline.Database.Entities.Photo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<bool>("Deleted")
                        .HasColumnType("boolean")
                        .HasColumnName("deleted");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("hash");

                    b.Property<int>("Height")
                        .HasColumnType("integer")
                        .HasColumnName("height");

                    b.Property<DateTimeOffset>("IntakeTake")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("intake_take");

                    b.Property<Point>("Location")
                        .HasColumnType("geometry")
                        .HasColumnName("location");

                    b.Property<string>("OriginalFileName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("original_file_name");

                    b.Property<string>("StoredPath")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("stored_path");

                    b.Property<DateTime>("Taken")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("taken");

                    b.Property<int>("Width")
                        .HasColumnType("integer")
                        .HasColumnName("width");

                    b.HasKey("Id")
                        .HasName("pk_photos");

                    b.ToTable("photos", (string)null);
                });

            modelBuilder.Entity("PhotoPipeline.Database.Entities.PhotoHash", b =>
                {
                    b.Property<Guid>("PhotoId")
                        .HasColumnType("uuid")
                        .HasColumnName("photo_id");

                    b.Property<string>("HashType")
                        .HasColumnType("text")
                        .HasColumnName("hash_type");

                    b.Property<string>("HashValue")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("hash_value");

                    b.HasKey("PhotoId", "HashType")
                        .HasName("pk_photo_hashes");

                    b.HasIndex("HashType", "HashValue")
                        .HasDatabaseName("ix_photo_hashes_hash_type_hash_value");

                    b.ToTable("photo_hashes", (string)null);
                });

            modelBuilder.Entity("PhotoPipeline.Database.Entities.PhotoMetadata", b =>
                {
                    b.Property<Guid>("PhotoId")
                        .HasColumnType("uuid")
                        .HasColumnName("photo_id");

                    b.Property<string>("Key")
                        .HasColumnType("text")
                        .HasColumnName("key");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("value");

                    b.HasKey("PhotoId", "Key")
                        .HasName("pk_photo_metadata");

                    b.HasIndex("Key", "Value")
                        .HasDatabaseName("ix_photo_metadata_key_value");

                    b.ToTable("photo_metadata", (string)null);
                });

            modelBuilder.Entity("PhotoPipeline.Database.Entities.RunPipelineStep", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Guid>("PhotoId")
                        .HasColumnType("uuid")
                        .HasColumnName("photo_id");

                    b.Property<DateTimeOffset>("Processed")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed");

                    b.Property<string>("StepName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("step_name");

                    b.Property<int>("StepVersion")
                        .HasColumnType("integer")
                        .HasColumnName("step_version");

                    b.HasKey("Id")
                        .HasName("pk_run_pipeline_step");

                    b.HasIndex("PhotoId")
                        .HasDatabaseName("ix_run_pipeline_step_photo_id");

                    b.ToTable("run_pipeline_step", (string)null);
                });

            modelBuilder.Entity("PhotoPipeline.Database.Entities.PhotoHash", b =>
                {
                    b.HasOne("PhotoPipeline.Database.Entities.Photo", "Photo")
                        .WithMany("Hashes")
                        .HasForeignKey("PhotoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_photo_hashes_photos_photo_id");

                    b.Navigation("Photo");
                });

            modelBuilder.Entity("PhotoPipeline.Database.Entities.PhotoMetadata", b =>
                {
                    b.HasOne("PhotoPipeline.Database.Entities.Photo", "Photo")
                        .WithMany("Metadata")
                        .HasForeignKey("PhotoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_photo_metadata_photos_photo_id");

                    b.Navigation("Photo");
                });

            modelBuilder.Entity("PhotoPipeline.Database.Entities.RunPipelineStep", b =>
                {
                    b.HasOne("PhotoPipeline.Database.Entities.Photo", "Photo")
                        .WithMany("PipelineSteps")
                        .HasForeignKey("PhotoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_run_pipeline_step_photos_photo_id");

                    b.Navigation("Photo");
                });

            modelBuilder.Entity("PhotoPipeline.Database.Entities.Photo", b =>
                {
                    b.Navigation("Hashes");

                    b.Navigation("Metadata");

                    b.Navigation("PipelineSteps");
                });
#pragma warning restore 612, 618
        }
    }
}
