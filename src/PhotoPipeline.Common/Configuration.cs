using Microsoft.Extensions.Configuration;

namespace PhotoPipeline.Common;

public class LocalStorageProvider
{
    public string Path { get; set; } = "";
}

public class Storage
{
    public string Provider { get; set; } = "";

    public LocalStorageProvider? Local { get; set; } = null;
}

public class DatabaseProvider
{
    public string Name { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public string ConnectionString { get; set; } = null!;
}

public class Database
{
    public string Default { get; set; } = null!;
    public DatabaseProvider[] Providers { get; set; } = Array.Empty<DatabaseProvider>();

    public DatabaseProvider GetDefaultProvider()
    {
        return Providers.First(provider => provider.Name == Default);
    }
}

public class PhotoPipelineConfig
{
    public Database Database { get; set; } = new();
    public Storage Storage { get; set; } = new();
    public string Environment { get; set; } = "";

    public int MaxParallelism { get; set; } = 1;
}

public static class ConfigurationProviderExtensions
{
    public static IConfigurationBuilder AddPhotoPipelineConfig(this IConfigurationBuilder builder, string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("PHOTOPIPELINE_ENVIRONMENT") ?? "development";
        Dictionary<string, string> extra = new()
        {
            { "PhotoPipeline:Environment", environment },
            { "PhotoPipeline:MaxParallelism", Environment.ProcessorCount.ToString() }
        };

        builder
            .AddInMemoryCollection(extra)
            .AddJsonFile("photopipeline.json")
            .AddJsonFile($"photopipeline.{environment}.json", true)
            .AddEnvironmentVariables("PHOTOPIPELINE")
            .AddCommandLine(args ?? Array.Empty<string>());

        return builder;
    }

    public static PhotoPipelineConfig GetPhotoPipelineConfig(this IConfiguration provider)
    {
        return provider.GetSection("PhotoPipeline").Get<PhotoPipelineConfig>();
    }
}