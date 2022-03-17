using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PhotoPipeline.Common;

namespace PhotoPipeline.Database;

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

        var provider = config.Database.GetDefaultProvider();

        return provider.Provider switch
        {
            "postgres" => throw new NotImplementedException("No Postgres yet"),
            "mssql" => (PhotoDbContext)new MsSqlPhotoDbContext(options.Options),
            _ => throw new Exception($"Unsupported provider: {config.Database.Default}")
        };
    }
}