using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PhotoPipeline.Common;

namespace PhotoPipeline.Database;

public class PostgresPhotoDbContextFactory : IDesignTimeDbContextFactory<PostgresPhotoDbContext>
{
    public PostgresPhotoDbContext CreateDbContext(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .AddPhotoPipelineConfig(args)
            .Build();

        var config = builder.GetPhotoPipelineConfig();
        Console.WriteLine(builder.GetDebugView());
        var options = new DbContextOptionsBuilder<PhotoDbContext>()
            .UseConfiguration(config);

        var provider = config.Database.GetDefaultProvider();
        return new PostgresPhotoDbContext(options.Options);
    }
}

public class PostgresPhotoDbContext : PhotoDbContext
{
    public PostgresPhotoDbContext(DbContextOptions<PhotoDbContext> options) : base(options)
    {
    }
}
