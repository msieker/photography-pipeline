using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PhotoPipeline.Common;
using PhotoPipeline.Database.Entities;
using System.Reflection;
using System.Text.Json;

#nullable disable

namespace PhotoPipeline.Database
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder<PhotoDbContext> UseConfiguration(this DbContextOptionsBuilder<PhotoDbContext> builder, PhotoPipelineConfig config)
        {
            var provider = config.Database.GetDefaultProvider();

            var _ = provider.Provider switch
            {
                //"postgres" => builder
                //.UseNpgsql(provider.ConnectionString, x => x.MigrationsAssembly("PhotoPipeline.Database.NpgSql"))
                //.UseSnakeCaseNamingConvention(),
                "mssql" => builder
                .UseSqlServer(provider.ConnectionString, x => x
                    .UseNetTopologySuite()),
                _ => throw new Exception($"Unsupported provider: {config.Database.Default}")
            };

            return builder;
        }
    }
    public class PhotoDbContextFactory : IDesignTimeDbContextFactory<PhotoDbContext>
    {
        public PhotoDbContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddPhotoPipelineConfig(args)
                .Build();

            var config = builder.GetPhotoPipelineConfig();
            Console.WriteLine(builder.GetDebugView());
            var options = new DbContextOptionsBuilder<PhotoDbContext>()
                .UseConfiguration(config);

            return new PhotoDbContext(options.Options);
        }
    }

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

        public DbSet<Photo> Photos { get; set; }
        public DbSet<PhotoMetadata> PhotoMetadata { get; set; }
        public DbSet<PhotoHash> PhotoHashes { get; set; }
    }
}