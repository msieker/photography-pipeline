using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

namespace PhotoPipeline.Database.Model.Entities;

public class Photo
{
    public Guid Id { get; set; }

    public string OriginalFileName { get; set; } = null!;
    public DateTimeOffset IntakeTake { get; set; } = DateTimeOffset.Now;
    public string Hash { get; set; } = "";

    public string StoredPath { get; set; } = "";

    public bool Deleted { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    public DateTime Taken { get; set; }
    public Point? Location { get; set; }

    public List<PhotoMetadata> Metadata { get; set; } = new();
    public List<PhotoHash> Hashes { get; set; } = new();

    public List<RunPipelineStep> PipelineSteps { get; set; } = new();
}

public class RunPipelineStep
{
    public int Id { get; set; }
    public Guid PhotoId { get; set; }
    public string StepName { get; set; } = "";
    public int StepVersion { get; set; }
    public DateTimeOffset Processed { get; set; } = DateTimeOffset.Now;

    public Photo Photo { get; set; } = default!;
}

public class PhotoMetadata
{
    public Guid PhotoId { get; set; }
    public Photo Photo { get; set; } = null!;

    public string Key { get; set; } = "";
    public string Value { get; set; } = "";

    internal class Configuration : IEntityTypeConfiguration<PhotoMetadata>
    {
        public void Configure(EntityTypeBuilder<PhotoMetadata> builder)
        {
            builder.HasKey(b => new { b.PhotoId, b.Key });
            builder.HasIndex(b => new { b.Key, b.Value });
        }
    }
}

public class PhotoHash
{
    public Guid PhotoId { get; set; }
    public Photo Photo { get; set; } = null!;

    public string HashType { get; set; } = "";
    public string HashValue { get; set; } = "";

    internal class Configuration : IEntityTypeConfiguration<PhotoHash>
    {
        public void Configure(EntityTypeBuilder<PhotoHash> builder)
        {
            builder.HasKey(b => new { b.PhotoId, b.HashType });

            builder.HasIndex(b => new { b.HashType, b.HashValue });
        }
    }
}