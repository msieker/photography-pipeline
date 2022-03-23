using Microsoft.Extensions.Logging;
using PhotoPipeline.Framework.Storage;

namespace PhotoPipeline.Framework.Blocks.Utility;

internal class RemoveFile : IPipelineBlock
{
    private ILogger<RemoveFile> _logger;
    private readonly StorageProvider _storageProvider;
    public RemoveFile(ILogger<RemoveFile> logger, StorageProvider storageProvider)
    {
        _logger = logger;
        _storageProvider = storageProvider;
    }

    public string BlockName => BlockNames.Utility.RemoveFile;
    public int BlockVersion => 1;

    public async Task<PipelinePhoto?> Run(PipelinePhoto photo, CancellationToken token)
    {
        var store = _storageProvider.GetFromPath(photo.OutputPath);
        if (!await store.Delete(photo, token)) return null;
        photo.Photo.Removed = true;
        return photo;

    }
}