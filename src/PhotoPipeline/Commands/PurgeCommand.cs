using Microsoft.Extensions.Logging;
using PhotoPipeline.Common;
using PhotoPipeline.Framework;

namespace PhotoPipeline.Commands;

internal class PurgeParameters
{

}

internal class PurgeHandler : ICommandHandler<PurgeParameters>
{
    private readonly ILogger<AddHandler> _logger;
    private readonly PhotoPipelineConfig _config;
    private readonly PipelineFactory _pipelineFactory;

    public PurgeHandler(ILogger<AddHandler> logger, PhotoPipelineConfig config, PipelineFactory pipelineFactory)
    {
        _logger = logger;
        _config = config;
        _pipelineFactory = pipelineFactory;
    }

    public async Task Handle(PurgeParameters args, CancellationToken token)
    {
        _logger.LogInformation("Deleting marked photos with parallelism of {parallelism}",  _config.MaxParallelism);
        await _pipelineFactory.Purge(token);
    }
}
