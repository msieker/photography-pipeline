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

    private ITargetBlock<PipelinePhoto> BuildPipeline(ISourceBlock<PipelinePhoto> source, string[] blocks, CancellationToken token = default)
    {
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        var saveToDatabase = _blocks.Output.SaveToDatabase(token);

        var final = new ActionBlock<PipelinePhoto>(p =>
        {
            _logger.LogDebug("Completed processing media: {mediaPath}", p.SourcePath);
            p.Dispose();
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _config.MaxParallelism, CancellationToken = token, EnsureOrdered = false });

        saveToDatabase.LinkTo(final, linkOptions);

        ISourceBlock<PipelinePhoto> lastHead = source;
        foreach (var step in blocks)
        {
            var block = _blocks.Get(step, token);
            lastHead.LinkTo(block, linkOptions);
            lastHead = block;
        }

        lastHead.LinkTo(saveToDatabase, linkOptions);

        return final;

    }

    public async Task Purge(CancellationToken token)
    {
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        var source = _blocks.Sources.FromDatabase(q => q.Where(p => p.Deleted && !p.Removed), token);

        var blockNames = new[]
        {
            BlockNames.Utility.RemoveFile
        };

        var final = BuildPipeline(source, blockNames, token);
        await source.RunQuery(token);
        try
        {
            await final.Completion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running pipeline");
        }
    }

    public async Task AzureVision(CancellationToken token)
    {
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        var source = _blocks.Sources.FromDatabase(q => q.Where(p => !p.Deleted), token);

        var blockNames = new[]
        {
            BlockNames.Processing.AzureVision,
            BlockNames.Utility.WriteFile
        };

        var final = BuildPipeline(source, blockNames, token);
        await source.RunQuery(token);
        try
        {
            await final.Completion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running pipeline");
        }
    }

    public async Task Add(string[] sourcePaths, CancellationToken token)
    {
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        var listDir = _blocks.Sources.ListDirectory(token);
        var readFile = _blocks.Sources.ReadFile(token);
        listDir.LinkTo(readFile, linkOptions);

        var blockNames = new[]
        {
            BlockNames.Processing.ResolveEntity,
            BlockNames.Utility.Deduplicate,
            BlockNames.Processing.HashFile,
            BlockNames.Processing.HashPerceptual,
            BlockNames.Processing.ReadExif,
            BlockNames.Utility.WriteFile
        };

        var final = BuildPipeline(readFile, blockNames, token);

        foreach (var p in sourcePaths)
        {
            await listDir.SendAsync(p, token);
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