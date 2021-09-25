using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

#nullable disable

namespace PhotoPipeline.Database.Entities
{
    public class Photo
    {
        public Guid Id { get; set; }

        public string OriginalFileName { get; set; } = null!;
        public DateTimeOffset IntakeTake { get; set; } = DateTimeOffset.Now;
        public string Hash { get; set; } = "";

        public int Width { get; set; }
        public int Height { get; set; }

        public DateTime Taken { get; set; }
        public Point? Location { get; set; }

        public List<PhotoMetadata> Metadata { get; set; } = new List<PhotoMetadata>();
        public List<PhotoHash> Hashes { get; set; } = new List<PhotoHash>();
    }

    public class PhotoMetadata
    {
        public Guid PhotoId { get; set; }
        public Photo Photo { get; set; }

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
        public Photo Photo { get; set; }

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
}
