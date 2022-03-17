//using System.Collections.Concurrent;
//using System.Threading.Tasks.Dataflow;
//using Microsoft.Extensions.Logging;
//using PhotoPipeline.Common;

//namespace PhotoPipeline.Framework;
//public class UtilityBlocks
//{
//    private readonly ILogger<ProcessingBlocks> _logger;
//    private readonly IServiceProvider _serviceProvider;
//    private readonly PhotoPipelineConfig _config;

//    public UtilityBlocks(ILogger<ProcessingBlocks> logger, IServiceProvider serviceProvider, PhotoPipelineConfig config)
//    {
//        _logger = logger;
//        _serviceProvider = serviceProvider;
//        _config = config;

//    }

//    private ExecutionDataflowBlockOptions MakeOptions(CancellationToken token) => new()
//    {
//        MaxDegreeOfParallelism = _config.MaxParallelism,
//        CancellationToken = token
//    };

//    public IPropagatorBlock<PipelinePhoto, PipelinePhoto> Deduplicate(CancellationToken token=default)
//    {
//        var dict = new ConcurrentDictionary<string, int>();

//        var source = new BufferBlock<PipelinePhoto>(MakeOptions(token));

//        var target = new ActionBlock<PipelinePhoto>(photo =>
//        {
//            if(photo.Exists) return;
            
//            var result = dict.AddOrUpdate(photo.Photo.Id, 0, (id, oldValue) => oldValue + 1);
//            if (result == 0)
//            {
//                source.Post(photo);
//            }
//            else
//            {
//                _logger.LogInformation("Duplicate image in import {imagePath}", photo.FileName);
//            }
//        }, MakeOptions(token));

//        target.Completion.ContinueWith(delegate
//        {
//            source.Complete();
//        }, token);
//        return DataflowBlock.Encapsulate(target, source);
//    }
//}
