// See https://aka.ms/new-console-template for more information
using CommandLine;
using Microsoft.Extensions.Configuration;
using PhotographyPipeline.Framework;
using System.Text.Json;

var configuration = new ConfigurationBuilder()
  .AddUserSecrets<Program>()
  .Build();

var result = Parser.Default.ParseArguments<PhotoIntakeOptions, AnalyzeOptions>(args);

await result.MapResult(
    async (PhotoIntakeOptions opt) => await PhotoIntakeOptions.PhotoIntakeProc(opt),
    async (AnalyzeOptions opt) => await AnalyzeOptions.AnalyzeProc(opt, configuration),
    errors => Task.FromResult(1));

//var fs = File.OpenRead()
//PhotoIntake.GetMetadata("")


[Verb("intake")]
public class PhotoIntakeOptions
{
    [Value(0, Required = true)]
    public string FileName { get; set; }

    public static async Task PhotoIntakeProc(PhotoIntakeOptions opts)
    {
        var fs = File.OpenRead(opts.FileName);
        var result = await PhotoIntake.GetMetadata(Path.GetFileName(opts.FileName), fs);

        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}

[Verb("analyze")]
public class AnalyzeOptions
{
    [Value(0, Required =true)]
    public string FileName { get; set; }

    public static async Task AnalyzeProc(AnalyzeOptions opts, IConfiguration configuration)
    {
        var endpoint = configuration["AZURE:VISION:ENDPOINT"];
        var key = configuration["AZURE:VISION:KEY"];
        var fs = File.OpenRead(opts.FileName);

        await Analyze.AzureAnalyze(fs, endpoint, key);
    }
}