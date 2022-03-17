using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using PhotoPipeline.Common;

namespace PhotoPipeline.Framework.Blocks;
public class SourceBlocks
{
    private readonly ILogger<SourceBlocks> _logger;
    private readonly PhotoPipelineConfig _config;

    public SourceBlocks(ILogger<SourceBlocks> logger, PhotoPipelineConfig config)
    {
        _logger = logger;
        _config = config;
    }
    private ExecutionDataflowBlockOptions MakeOptions(CancellationToken token) => new()
    {
        MaxDegreeOfParallelism = _config.MaxParallelism,
        CancellationToken = token
    };

    public TransformManyBlock<string, string> ListDirectory(CancellationToken token = default)
        => new(path => Sources.ListDirectory.Action(path, _logger, token),
            new ExecutionDataflowBlockOptions {CancellationToken = token});

    public TransformBlock<string, PipelinePhoto> ReadFile(CancellationToken token = default)
        => new(path => Sources.ReadFile.Action(path, _logger, token),
            MakeOptions(token));
}
