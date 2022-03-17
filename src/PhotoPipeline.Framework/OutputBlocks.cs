//using System.Collections.Concurrent;
//using System.Threading.Tasks.Dataflow;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using PhotoPipeline.Common;
//using PhotoPipeline.Database;
//using PhotoPipeline.Database.Entities;

//namespace PhotoPipeline.Framework;

//public class OutputBlocks
//{
//    private readonly IServiceProvider _serviceProvider;
//    private readonly ILogger<OutputBlocks> _logger;
//    private readonly PhotoPipelineConfig _config;

//    public ConcurrentBag<Photo> Photos { get; } = new();

//    public OutputBlocks(IServiceProvider serviceProvider, ILogger<OutputBlocks> logger, PhotoPipelineConfig config)
//    {
//        _serviceProvider = serviceProvider;
//        _logger = logger;
//        _config = config;
//    }

//    private ExecutionDataflowBlockOptions MakeOptions(CancellationToken token) => new()
//    {
//        MaxDegreeOfParallelism = _config.MaxParallelism,
//        CancellationToken = token
//    };

//    public TransformBlock<PipelinePhoto, PipelinePhoto> SaveToList(CancellationToken token = default)
//    {
//        return new TransformBlock<PipelinePhoto, PipelinePhoto>(photo =>
//        {
//            Photos.Add(photo.Photo);
//            return photo;
//        }, MakeOptions(token));
//    }

//    public TransformBlock<PipelinePhoto, PipelinePhoto> CopyFile(CancellationToken token = default)
//    {
//        return new TransformBlock<PipelinePhoto, PipelinePhoto>(async photo =>
//        {
//            if (photo.Memory == null)
//            {
//                _logger.LogWarning("Got a photo with no data");
//                return photo;
//            }
//            var basePath = Path.Combine(_config.Storage.Local!.Path, photo.Photo.Taken.ToString("yyyy"), photo.Photo.Taken.ToString("MM"), photo.Photo.Taken.ToString("dd"));

//            Directory.CreateDirectory(basePath);

//            var written = false;
//            var filePath = Path.Combine(basePath, photo.FileName);
//            var hashSuffixSize = 1;
//            while (!written)
//            {
//                try
//                {
//                    await using var outFile = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true);
//                    await outFile.WriteAsync(photo.Memory.Value, token);
//                    written = true;
//                    photo.Photo.StoredPath = filePath;
//                }
//                catch (IOException)
//                {
//                    var newFileName = string.Concat(Path.GetFileNameWithoutExtension(photo.FileName), "-", photo.Id.AsSpan(0, hashSuffixSize), Path.GetExtension(photo.FileName));
//                    filePath = Path.Join(basePath, newFileName);
//                    written = false;
//                    hashSuffixSize++;
//                }

//                if (hashSuffixSize > photo.Id.Length)
//                {
//                    _logger.LogError("Ran out of hash bytes when trying to copy {fileName} ({hash}) to path {path}", photo.FileName, photo.Id, basePath);
//                    photo.Errored = true;
//                    break;
//                }
//            }
//            return photo;
//        }, MakeOptions(token));
//    }

//    public IPropagatorBlock<PipelinePhoto, PipelinePhoto> SaveToDatabase(CancellationToken token = default)
//    {
//        var batchBlock = new BatchBlock<PipelinePhoto>(100);

//        var source = new BufferBlock<PipelinePhoto>();

//        var target = new ActionBlock<PipelinePhoto>(photo =>
//        {
//            if (photo.Errored) return;
//            batchBlock.Post(photo);
//            source.Post(photo);
//        }, MakeOptions(token));

//        var actionBlock = new ActionBlock<PipelinePhoto[]>(async (photos) =>
//        {
//            _logger.LogInformation("Saving batch of {photoCount} photos", photos.Length);
//            await using var scope = _serviceProvider.CreateAsyncScope();
//            var context = scope.ServiceProvider.GetRequiredService<PhotoDbContext>();

//            await context.Photos.AddRangeAsync(photos.Select(p => p.Photo), token);
//            //await context.SaveChangesAsync(token);
//        }, MakeOptions(token));

//        batchBlock.LinkTo(actionBlock);

//        target.Completion.ContinueWith(delegate
//        {
//            batchBlock.Complete();
//        }, token);
//        batchBlock.Completion.ContinueWith(delegate
//        {
//            actionBlock.Complete();
//        }, token);
//        actionBlock.Completion.ContinueWith(delegate
//        {
//            source.Complete();
//        }, token);
//        return DataflowBlock.Encapsulate(target, source);
//    }
//}
