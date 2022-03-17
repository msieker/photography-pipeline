using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PhotoPipeline.Common;

namespace PhotoPipeline.Database;

public static class DbContextOptionsBuilderExtensions
{
    public static IServiceCollection AddPhotoDbContext(this IServiceCollection services, PhotoPipelineConfig config)
    {
        var provider = config.Database.GetDefaultProvider();
        var optBuilder = new DbContextOptionsBuilder<PhotoDbContext>();

        optBuilder.UseConfiguration(config);

        var options = optBuilder.Options;
        services.AddSingleton(options);
        switch (provider.Provider)
        {
            case "mssql":
                services.AddDbContext<PhotoDbContext, MsSqlPhotoDbContext>(opt =>
                    opt.UseSqlServer(provider.ConnectionString, x => x.UseNetTopologySuite()));
                break;
            case "postgres":
                services.AddDbContext<PhotoDbContext, PostgresPhotoDbContext>(opt =>
                    opt.UseNpgsql(provider.ConnectionString, x => x.UseNetTopologySuite()).UseSnakeCaseNamingConvention());
                break;
            default:
                throw new Exception($"Unsupported provider: {config.Database.Default}");
        }

        return services;
    }

    public static DbContextOptionsBuilder<PhotoDbContext> UseConfiguration(this DbContextOptionsBuilder<PhotoDbContext> builder, PhotoPipelineConfig config)
    {
        var provider = config.Database.GetDefaultProvider();

        var _ = provider.Provider switch
        {
            "postgres" => builder
                .UseNpgsql(provider.ConnectionString, x => x.UseNetTopologySuite())
                .UseSnakeCaseNamingConvention(),
            "mssql" => builder
                .UseSqlServer(provider.ConnectionString, x => x
                .UseNetTopologySuite()),
            _ => throw new Exception($"Unsupported provider: {config.Database.Default}")
        };

        return builder;
    }
}

