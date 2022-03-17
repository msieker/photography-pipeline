using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PhotoPipeline.Database.Entities;

namespace PhotoPipeline.Database;

public class PhotoDbContext : DbContext
{
    public PhotoDbContext(DbContextOptions<PhotoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<PhotoMetadata> PhotoMetadata => Set<PhotoMetadata>();
    public DbSet<PhotoHash> PhotoHashes => Set<PhotoHash>();
    public DbSet<PhotoPipelineStep> PhotoPipelineStep => Set<PhotoPipelineStep>();
}
