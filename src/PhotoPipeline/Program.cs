using CommandLine;
using Microsoft.Extensions.Configuration;
using PhotographyPipeline.Framework;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PhotoPipeline.Common;
using PhotoPipeline.Database;

var builder = new ConfigurationBuilder()
    .AddPhotoPipelineConfig(args)
    .Build();

var config = builder.GetPhotoPipelineConfig();
Console.WriteLine(builder.GetDebugView());
var options = new DbContextOptionsBuilder<PhotoDbContext>()
    .UseConfiguration(config);


var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var result = Parser.Default.ParseArguments<PhotoIntakeOptions, object>(args);

await result.MapResult(
    async (PhotoIntakeOptions opt) => await PhotoIntakeOptions.PhotoIntakeProc(opt),
    errors => Task.FromResult(1));

//var fs = File.OpenRead()
//PhotoIntake.GetMetadata("")


[Verb("add")]
public class PhotoIntakeOptions
{
    [Value(0, Required = true)]
    public string FileName { get; set; }

    public static async Task PhotoIntakeProc(PhotoIntakeOptions opts)
    {
        var attr = File.GetAttributes(opts.FileName);
        if
        var fs = File.OpenRead(opts.FileName);
        var result = await PhotoIntake.GetMetadata(Path.GetFileName(opts.FileName), fs);
        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}