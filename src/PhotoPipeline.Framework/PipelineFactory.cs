using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using PhotoPipeline.Common;
using PhotoPipeline.Framework.Blocks;

namespace PhotoPipeline.Framework;

public class PipelineFactory
{
    private readonly ILogger<PipelineFactory> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PhotoPipelineConfig _config;
    private readonly BlockFactory _blocks;

    public PipelineFactory(ILogger<PipelineFactory> logger, IServiceProvider serviceProvider, PhotoPipelineConfig config, BlockFactory blocks)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _config = config;
        _blocks = blocks;
    }

    public async Task Add(string[] sourcePaths, CancellationToken token)
    {
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        var listDir = _blocks.Sources.ListDirectory(token);
        var readFile = _blocks.Sources.ReadFile(token);

        var saveToDatabase = _blocks.Output.SaveToDatabase(token);
        var copyFile = _blocks.Output.CopyFile(token);

        var final = new ActionBlock<PipelinePhoto>(p =>
        {
            _logger.LogInformation("Completed processing media: {mediaPath}", p.SourcePath);
            p.Dispose();
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _config.MaxParallelism, CancellationToken = token });

        listDir.LinkTo(readFile, linkOptions);

        saveToDatabase.LinkTo(copyFile, linkOptions);
        copyFile.LinkTo(final, linkOptions);
        var blockNames = new[]
        {
            BlockNames.Processing.ResolveEntity,
            BlockNames.Utility.Deduplicate,
            BlockNames.Processing.HashFile,
            BlockNames.Processing.HashPerceptual,
            BlockNames.Processing.ReadExif
        };
        
        ISourceBlock<PipelinePhoto> lastHead = readFile;
        foreach (var step in blockNames)
        {
            var block = _blocks.Get(step, token);
            lastHead.LinkTo(block, linkOptions);
            lastHead = block;
        }

        lastHead.LinkTo(saveToDatabase, linkOptions);

        foreach (var p in sourcePaths)
        {
            listDir.Post(p);
        }
        listDir.Complete();

        try
        {
            await final.Completion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running pipeline");
        }
    }
}