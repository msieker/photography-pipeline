using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PhotoPipeline.Common;

namespace PhotoPipeline.Database;

public class MsSqlPhotoDbContextFactory : IDesignTimeDbContextFactory<MsSqlPhotoDbContext>
{
    public MsSqlPhotoDbContext CreateDbContext(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .AddPhotoPipelineConfig(args)
            .Build();

        var config = builder.GetPhotoPipelineConfig();
        Console.WriteLine(builder.GetDebugView());
        var options = new DbContextOptionsBuilder<PhotoDbContext>()
            .UseConfiguration(config);

        var provider = config.Database.GetDefaultProvider();
        return new MsSqlPhotoDbContext(options.Options);
    }
}

public class MsSqlPhotoDbContext : PhotoDbContext
{
    public MsSqlPhotoDbContext(DbContextOptions<PhotoDbContext> options) : base(options)
    {
    }
}
