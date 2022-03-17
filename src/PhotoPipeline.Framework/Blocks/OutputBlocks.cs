using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
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
        CancellationToken = token
    };

    public TransformBlock<PipelinePhoto, PipelinePhoto> SaveToList(CancellationToken token = default)
    {
        return new TransformBlock<PipelinePhoto, PipelinePhoto>(photo =>
        {
            Photos.Add(photo.Photo);
            return photo;
        }, MakeOptions(token));
    }

    public TransformBlock<PipelinePhoto, PipelinePhoto> CopyFile(CancellationToken token = default)
    {
        return new TransformBlock<PipelinePhoto, PipelinePhoto>(async photo =>
        {
            if (photo.Memory == null)
            {
                _logger.LogWarning("Got a photo with no data");
                return photo;
            }
            var basePath = Path.Combine(_config.Storage.Local!.Path, photo.Photo.Taken.ToString("yyyy"), photo.Photo.Taken.ToString("MM"), photo.Photo.Taken.ToString("dd"));

            Directory.CreateDirectory(basePath);

            var written = false;
            var filePath = Path.Combine(basePath, photo.FileName);
            var hashSuffixSize = 1;
            while (!written)
            {
                try
                {
                    await using var outFile = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true);
                    await outFile.WriteAsync(photo.Memory.Value, token);
                    written = true;
                    photo.Photo.StoredPath = filePath;
                    photo.OutputPath = filePath;
                    await photo.WriteJson(token);
                }
                catch (IOException)
                {
                    var newFileName = string.Concat(Path.GetFileNameWithoutExtension(photo.FileName), "-", photo.Id.AsSpan(0, hashSuffixSize), Path.GetExtension(photo.FileName));
                    filePath = Path.Join(basePath, newFileName);
                    written = false;
                    hashSuffixSize++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error writing data for {fileName} ({hash}) to path {path}", photo.FileName, photo.Id, basePath);
                    throw;
                }

                if (hashSuffixSize > photo.Id.Length)
                {
                    _logger.LogError("Ran out of hash bytes when trying to copy {fileName} ({hash}) to path {path}", photo.FileName, photo.Id, basePath);
                    photo.Errored = true;
                    break;
                }
            }
            return photo;
        }, MakeOptions(token));
    }

    public IPropagatorBlock<PipelinePhoto, PipelinePhoto> SaveToDatabase(CancellationToken token = default)
    {
        var batchBlock = new BatchBlock<PipelinePhoto>(100);

        var source = new BufferBlock<PipelinePhoto>();

        var target = new ActionBlock<PipelinePhoto>(photo =>
        {
            if (photo.Errored) return;
            batchBlock.Post(photo);
            source.Post(photo);
        }, MakeOptions(token));

        var actionBlock = new ActionBlock<PipelinePhoto[]>(async (photos) =>
        {
            _logger.LogInformation("Saving batch of {photoCount} photos", photos.Length);
            await using var scope = _serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<PhotoDbContext>();

            var photoList = photos.Select(p => p.Photo).ToList();
            var metadata = photoList.SelectMany(p => p.Metadata, (p, m) => new PhotoMetadata {Key = m.Key, PhotoId = p.Id, Source = m.Source, Value = m.Value});
            var hashes = photoList.SelectMany(p => p.Hashes, (p, h) => new PhotoHash { PhotoId = p.Id, Source = h.Source, HashType = h.HashType, HashValue = h.HashValue});
            var steps = photoList.SelectMany(p => p.PipelineSteps, (p, s) => new PhotoPipelineStep { PhotoId = p.Id, StepName  = s.StepName, StepVersion = s.StepVersion, Processed = s.Processed});
            
            //await context.Photos.UpsertRange(photos.Select(p=>p.Photo))
            //    .On(p => p.Id)
            //    .RunAsync(token);

            //await context.PhotoMetadata.UpsertRange(metadata)
            //    .On(m => new {m.PhotoId, m.Key})
            //    .RunAsync(token);

            //await context.PhotoHashes.UpsertRange(hashes)
            //    .On(h => new {h.PhotoId, h.HashType})
            //    .RunAsync(token);

            //await context.PhotoPipelineStep.UpsertRange(steps)
            //    .On(s => new {s.PhotoId, s.StepName})
            //    .RunAsync(token);

        }, MakeOptions(token));

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
