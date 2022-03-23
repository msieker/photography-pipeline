using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

namespace PhotoPipeline.Database.Entities;

public class Photo
{
    public string Id { get; set; } = "";

    public string OriginalPath { get; set; } = "";
    public string OriginalFileName { get; set; } = "";
    public DateTimeOffset IntakeTake { get; set; } = DateTimeOffset.Now;

    public string StoredPath { get; set; } = "";

    public bool Removed { get; set; }
    public bool Deleted { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    public DateTime Taken { get; set; }
    public Point? Location { get; set; }

    public List<PhotoMetadata> Metadata { get; set; } = new();
    public List<PhotoHash> Hashes { get; set; } = new();

    public List<PhotoPipelineStep> PipelineSteps { get; set; } = new();

    internal class Configuration : IEntityTypeConfiguration<Photo>
    {
        public void Configure(EntityTypeBuilder<Photo> builder)
        {
            builder.Property(b => b.Id)
                .HasMaxLength(64).IsFixedLength().IsUnicode(false);
        }
    }
}

public class PhotoPipelineStep
{
    public string PhotoId { get; set; } = null!;
    public string StepName { get; set; } = "";
    public int StepVersion { get; set; }
    public DateTimeOffset Processed { get; set; } = DateTimeOffset.Now;

    public Photo Photo { get; set; } = default!;

    internal class Configuration : IEntityTypeConfiguration<PhotoPipelineStep>
    {
        public void Configure(EntityTypeBuilder<PhotoPipelineStep> builder)
        {
            builder.HasKey(b => new { b.PhotoId, b.StepName });
            builder.Property(b => b.PhotoId)
                .HasMaxLength(64).IsFixedLength().IsUnicode(false); ;
        }
    }
}

public class PhotoMetadata
{
    public string PhotoId { get; set; } = "";
    public Photo Photo { get; set; } = null!;

    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string Source { get; set; } = "";

    internal class Configuration : IEntityTypeConfiguration<PhotoMetadata>
    {
        public void Configure(EntityTypeBuilder<PhotoMetadata> builder)
        {
            builder.Property(b => b.PhotoId)
                .HasMaxLength(64).IsFixedLength().IsUnicode(false);
            builder.HasKey(b => new { b.PhotoId, b.Key });
        }
    }
}

public class PhotoHash
{
    public string PhotoId { get; set; } = "";
    public Photo Photo { get; set; } = null!;

    public string HashType { get; set; } = "";
    public string HashValue { get; set; } = "";

    public string Source { get; set; } = "";

    internal class Configuration : IEntityTypeConfiguration<PhotoHash>
    {
        public void Configure(EntityTypeBuilder<PhotoHash> builder)
        {
            builder.Property(b => b.PhotoId)
                .HasMaxLength(64).IsFixedLength().IsUnicode(false);
            builder.HasKey(b => new { b.PhotoId, b.HashType });

            builder.HasIndex(b => new { b.HashType, b.HashValue });
        }
    }
}