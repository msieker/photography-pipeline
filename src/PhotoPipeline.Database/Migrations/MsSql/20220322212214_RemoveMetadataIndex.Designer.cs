﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using PhotoPipeline.Database;

#nullable disable

namespace PhotoPipeline.Database.Migrations.mssql
{
    [DbContext(typeof(MsSqlPhotoDbContext))]
    [Migration("20220322212214_RemoveMetadataIndex")]
    partial class RemoveMetadataIndex
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("PhotoPipeline.Database.Entities.Photo", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(64)
                        .IsUnicode(false)
                        .HasColumnType("char(64)")
                        .IsFixedLength();

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<int>("Height")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("IntakeTake")
                        .HasColumnType("datetimeoffset");

                    b.Property<Point>("Location")
                        .HasColumnType("geography");

                    b.Property<string>("OriginalFileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OriginalPath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Removed")
                        .HasColumnType("bit");

                    b.Property<string>("StoredPath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Taken")
                        .HasColumnType("datetime2");

                    b.Property<int>("Width")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Photos");
                });

            modelBuilder.Entity("PhotoPipeline.Database.Entities.PhotoHash", b =>
                {
                    b.Property<string>("PhotoId")
                        .HasMaxLength(64)
                        .IsUnicode(false)
                        .HasColumnType("char(64)")
                        .IsFixedLength();

                    b.Property<string>("HashType")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("HashValue")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PhotoId", "HashType");

                    b.HasIndex("HashType", "HashValue");

                    b.ToTable("PhotoHashes");
                });

            modelBuilder.Entity("PhotoPipeline.Database.Entities.PhotoMetadata", b =>
                {
                    b.Property<string>("PhotoId")
                        .HasMaxLength(64)
                        .IsUnicode(false)
                        .HasColumnType("char(64)")
                        .IsFixedLength();

                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PhotoId", "Key");

                    b.ToTable("PhotoMetadata");
                });

            modelBuilder.Entity("PhotoPipeline.Database.Entities.PhotoPipelineStep", b =>
                {
                    b.Property<string>("PhotoId")
                        .HasMaxLength(64)
                        .IsUnicode(false)
                        .HasColumnType("char(64)")
                        .IsFixedLength();

                    b.Property<string>("StepName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("Processed")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("StepVersion")
                        .HasColumnType("int");

                    b.HasKey("PhotoId", "StepName");

                    b.ToTable("PhotoPipelineStep");
                });

            modelBuilder.Entity("PhotoPipeline.Database.Entities.PhotoHash", b =>
                {
                    b.HasOne("PhotoPipeline.Database.Entities.Photo", "Photo")
                        .WithMany("Hashes")
                        .HasForeignKey("PhotoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Photo");
                });

            modelBuilder.Entity("PhotoPipeline.Database.Entities.PhotoMetadata", b =>
                {
                    b.HasOne("PhotoPipeline.Database.Entities.Photo", "Photo")
                        .WithMany("Metadata")
                        .HasForeignKey("PhotoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Photo");
                });

            modelBuilder.Entity("PhotoPipeline.Database.Entities.PhotoPipelineStep", b =>
                {
                    b.HasOne("PhotoPipeline.Database.Entities.Photo", "Photo")
                        .WithMany("PipelineSteps")
                        .HasForeignKey("PhotoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

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
