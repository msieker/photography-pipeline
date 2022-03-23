using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PhotoPipeline.Common;
using PhotoPipeline.Framework;

namespace PhotoPipeline.Commands;
internal class VisionParameters
{

}

internal class VisionHandler : ICommandHandler<VisionParameters>
{
    private readonly ILogger<AddHandler> _logger;
    private readonly PhotoPipelineConfig _config;
    private readonly PipelineFactory _pipelineFactory;

    public VisionHandler(ILogger<AddHandler> logger, PhotoPipelineConfig config, PipelineFactory pipelineFactory)
    {
        _logger = logger;
        _config = config;
        _pipelineFactory = pipelineFactory;
    }

    public async Task Handle(VisionParameters args, CancellationToken token)
    {
        _logger.LogInformation("Running Azure Vision");
        await _pipelineFactory.AzureVision(token);
    }
}
