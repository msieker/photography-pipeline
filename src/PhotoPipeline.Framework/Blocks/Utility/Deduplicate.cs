using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace PhotoPipeline.Framework.Blocks.Utility;
internal class Deduplicate : IPipelineBlock
{
    private readonly ILogger<Deduplicate> _logger;

    public Deduplicate(ILogger<Deduplicate> logger)
    {
        _logger = logger;
    }

    public string BlockName => BlockNames.Utility.Deduplicate;
    public int BlockVersion => 1;

    private readonly ConcurrentDictionary<string, int> _seen = new();

    public Task<PipelinePhoto?> Run(PipelinePhoto photo, CancellationToken token)
    {
        var result = _seen.AddOrUpdate(photo.Photo.Id, 0, (id, oldValue) => oldValue + 1);
        if (result == 0)
        {
            return Task.FromResult(photo)!;
        }
        else
        {
            _logger.LogDebug("Duplicate image in import {imagePath}", photo.FileName);
            return Task.FromResult((PipelinePhoto?)null)!;
        }
    }
}
