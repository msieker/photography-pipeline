using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoPipeline.Common;
using PhotoPipeline.Database.Entities;

namespace PhotoPipeline.Framework.Blocks;

public class BlockFactory
{
    private readonly ILogger<BlockFactory> _logger;
    private readonly SourceBlocks _source;
    private readonly OutputBlocks _output;
    private readonly Dictionary<string, IPipelineBlock> _blocks;
    private readonly PhotoPipelineConfig _config;

    public BlockFactory(SourceBlocks source, OutputBlocks output, IEnumerable<IPipelineBlock> blocks, PhotoPipelineConfig config, ILogger<BlockFactory> logger)
    {
        _source = source;
        _output = output;
        _config = config;
        _logger = logger;
        _blocks = blocks.ToDictionary(k => k.BlockName, v => v);
    }

    public SourceBlocks Sources => _source;
    public OutputBlocks Output => _output;

    private ExecutionDataflowBlockOptions MakeOptions(CancellationToken token) => new()
    {
        MaxDegreeOfParallelism = _config.MaxParallelism,
        BoundedCapacity = _config.MaxParallelism * 2,
        CancellationToken = token,
        EnsureOrdered = false
    };

    public IPropagatorBlock<PipelinePhoto, PipelinePhoto> Get(string blockName, CancellationToken token)
    {
        if (!_blocks.TryGetValue(blockName, out var block))
        {
            throw new Exception($"Unknown block type {blockName}");
        }

        var source = new BufferBlock<PipelinePhoto>(MakeOptions(token));
        var target = new ActionBlock<PipelinePhoto>(async photo =>
        {
            PipelinePhoto? result;
            try
            {
                result = await block.Run(photo, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running block {blockName} on photo {photoPath}", blockName, photo.SourcePath);
                throw;
            }

            if (result == null) return;

            var record = photo.Photo.PipelineSteps.FirstOrDefault(s => s.StepName == block.BlockName);
            if (record == null)
            {
                record = new PhotoPipelineStep
                {
                    StepName = block.BlockName
                };
                photo.Photo.PipelineSteps.Add(record);
            }

            record.StepVersion = block.BlockVersion;
            record.Processed = DateTimeOffset.Now;

            await source.SendAsync(result, token);
        }, MakeOptions(token));

        target.Completion.ContinueWith(delegate { source.Complete(); }, token);

        return DataflowBlock.Encapsulate(target, source);
    }
}

public static class BlockFactoryExtensions
{
    public static IServiceCollection RegisterBlocks(this IServiceCollection services)
    {
        services.AddPipelineBlocks();
        services.AddTransient<BlockFactory>();
        services.AddTransient<SourceBlocks>();
        services.AddTransient<OutputBlocks>();
        return services;
    }
}