using Microsoft.Extensions.DependencyInjection;
using PhotoPipeline.Common;

namespace PhotoPipeline.Framework.Storage;

public interface IStorageProvider
{
    string Name { get; }

    Task<bool> Save(PipelinePhoto photo, CancellationToken token);
    Task<bool> Delete(PipelinePhoto photo, CancellationToken token);
    Task<bool> Read(PipelinePhoto photo, CancellationToken token);

    bool CanHandle(string path);
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStorageProviders(this IServiceCollection services, PhotoPipelineConfig config)
    {
        if (config.Storage.Local != null && config.Storage.Local.IsConfigured())
        {
            services.AddTransient<IStorageProvider, LocalFilesystemStorageProvider>();
        }

        services.AddSingleton<StorageProvider>();
        return services;
    }
}

public class StorageProvider
{
    private readonly IDictionary<string, IStorageProvider> _providers;
    private readonly IStorageProvider _defaultProvider;

    public StorageProvider(IEnumerable<IStorageProvider> providers, PhotoPipelineConfig config)
    {
        _providers = providers.ToDictionary(k => k.Name, v => v);

        if (_providers.TryGetValue(config.Storage.Provider, out var defaultProvider))
        {
            _defaultProvider = defaultProvider;
        }
        else
        {
            throw new InvalidOperationException($"Unconfigured default storage provider {config.Storage.Provider}");
        }
    }

    public IStorageProvider GetFromPath(string path)
    {
        foreach (var (_, p) in _providers)
        {
            if (p.CanHandle(path)) return p;
        }

        throw new InvalidOperationException($"Trying to resolve a file no one can handle: '{path}'");
    }

    public IStorageProvider Default => _defaultProvider;
    public IStorageProvider this[string name] => _providers[name];
}