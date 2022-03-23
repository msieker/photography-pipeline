using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhotographyPipeline.Framework;
using PhotoPipeline.Common;
using PhotoPipeline.Framework;

namespace PhotoPipeline.Commands;

internal class AddParameters
{
    public string[] Path { get; set; } = null!;
}

internal class AddHandler : ICommandHandler<AddParameters>
{
    private readonly ILogger<AddHandler> _logger;
    private readonly PhotoPipelineConfig _config;
    private readonly PipelineFactory _pipelineFactory;

    public AddHandler(ILogger<AddHandler> logger, PhotoPipelineConfig config, PipelineFactory pipelineFactory)
    {
        _logger = logger;
        _config = config;
        _pipelineFactory = pipelineFactory;
    }

    public async Task Handle(AddParameters args, CancellationToken token)
    {
        _logger.LogInformation("Processing new files in {sourcePath} with parallelism of {parallelism}", args.Path, _config.MaxParallelism);
        await _pipelineFactory.Add(args.Path, token);
    }
}