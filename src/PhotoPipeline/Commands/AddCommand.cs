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

internal class AddCommand : Command
{
    public AddCommand() : base("add", "Adds photos")
    {
        var path = new Option<string[]>("--path") { AllowMultipleArgumentsPerToken = true, IsRequired = true };

        AddOption(path);
        
        this.Handler = CommandHandler.Create<AddParameters, IHost, CancellationToken>(Process);
    }

    public async Task Process(AddParameters args, IHost host, CancellationToken token)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<AddHandler>();
        await handler.Handle(args, token);
        //var attr = File.GetAttributes(args.Path);

        //var fs = File.OpenRead(args.Path);
        //var result = await PhotoIntake.GetMetadata(Path.GetFileName(args.Path), fs);
        //Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
        //{
        //    WriteIndented = true
        //}));
    }
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