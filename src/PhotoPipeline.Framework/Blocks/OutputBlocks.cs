using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using LinqToDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoPipeline.Common;
using PhotoPipeline.Database;
using PhotoPipeline.Database.Entities;

namespace PhotoPipeline.Framework.Blocks;
public class OutputBlocks
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutputBlocks> _logger;
    private readonly PhotoPipelineConfig _config;

    public ConcurrentBag<Photo> Photos { get; } = new();

    public OutputBlocks(IServiceProvider serviceProvider, ILogger<OutputBlocks> logger, PhotoPipelineConfig config)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _config = config;
    }

    private ExecutionDataflowBlockOptions MakeOptions(CancellationToken token) => new()
    {
        MaxDegreeOfParallelism = _config.MaxParallelism,
        EnsureOrdered = false,
        CancellationToken = token
    };
    
    public IPropagatorBlock<PipelinePhoto, PipelinePhoto> SaveToDatabase(CancellationToken token = default)
    {
        var batchBlock = new BatchBlock<PipelinePhoto>(100);

        var source = new BufferBlock<PipelinePhoto>();

        var target = new ActionBlock<PipelinePhoto>(async photo =>
        {
            if (photo.Errored) return;
            await batchBlock.SendAsync(photo, token);
            await source.SendAsync(photo, token);
        }, MakeOptions(token));

        var actionBlock = new ActionBlock<PipelinePhoto[]>(async (photos) =>
        {
            try
            {
                _logger.LogInformation("Saving batch of {photoCount} photos", photos.Length);
                await using var scope = _serviceProvider.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<PhotoDbContext>();

                var photoList = photos.Select(p => p.Photo).ToList();
                var metadata = photoList.SelectMany(p => p.Metadata, (p, m) => new PhotoMetadata { Key = m.Key, PhotoId = p.Id, Source = m.Source, Value = m.Value });
                var hashes = photoList.SelectMany(p => p.Hashes, (p, h) => new PhotoHash { PhotoId = p.Id, Source = h.Source, HashType = h.HashType, HashValue = h.HashValue });
                var steps = photoList.SelectMany(p => p.PipelineSteps, (p, s) => new PhotoPipelineStep { PhotoId = p.Id, StepName = s.StepName, StepVersion = s.StepVersion, Processed = s.Processed });

                //await context.Photos.Merge()
                //    .Using(photoList)
                //    .OnTargetKey()
                //    .InsertWhenNotMatched()
                //    .UpdateWhenMatched()
                //    .MergeAsync(token);

                await context.Photos.UpsertRange(photos.Select(p => p.Photo))
                    .On(p => p.Id)
                    .RunAsync(token);

                await context.PhotoMetadata.UpsertRange(metadata)
                    .On(m => new { m.PhotoId, m.Key })
                    .RunAsync(token);

                await context.PhotoHashes.UpsertRange(hashes)
                    .On(h => new { h.PhotoId, h.HashType })
                    .RunAsync(token);

                await context.PhotoPipelineStep.UpsertRange(steps)
                    .On(s => new { s.PhotoId, s.StepName })
                    .RunAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing to database");
                throw;
            }

        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, EnsureOrdered = false, CancellationToken = token });

        batchBlock.LinkTo(actionBlock);

        target.Completion.ContinueWith(delegate
        {
            batchBlock.Complete();
        }, token);
        batchBlock.Completion.ContinueWith(delegate
        {
            actionBlock.Complete();
        }, token);
        actionBlock.Completion.ContinueWith(delegate
        {
            source.Complete();
        }, token);
        return DataflowBlock.Encapsulate(target, source);
    }
}
