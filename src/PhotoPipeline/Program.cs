using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using LinqToDB.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using PhotoPipeline.Commands;
using PhotoPipeline.Common;
using PhotoPipeline.Database;
using PhotoPipeline.Framework;
using PhotoPipeline.Framework.Blocks;
using PhotoPipeline.Framework.Storage;

namespace PhotoPipeline;

//var builder = new ConfigurationBuilder()
//    .AddPhotoPipelineConfig(args)
//    .Build();

//var config = builder.GetPhotoPipelineConfig();
//Console.WriteLine(builder.GetDebugView());
//var options = new DbContextOptionsBuilder<PhotoDbContext>()
//    .UseConfiguration(config);

//var configuration = new ConfigurationBuilder()
//    .AddUserSecrets<Program>()
//    .Build();



//rootCommand.InvokeAsync(args);

public static class Program
{
    public static async Task Main(string[] args)
    {
        var host = BuildCommandLine()
            .UseHost(_ => Host.CreateDefaultBuilder(), host => ConfigureHost(host, args))
            .UseDefaults()
            .Build();

        await host.InvokeAsync(args);
    }

    private static void ConfigureHost(IHostBuilder host, string[] args)
    {
        host.ConfigureAppConfiguration(cb =>
        {
            cb.AddPhotoPipelineConfig(args);
            
        });

        host.ConfigureServices((hb, services) =>
        {
            
            var config = hb.Configuration.GetPhotoPipelineConfig();
            
            services.AddSingleton(config);
            services.AddPhotoDbContext(config);
            services.RegisterBlocks();
            services.AddScoped<PipelineFactory>();
            services.AddCommandHandlers();
            services.AddStorageProviders(config);
            LinqToDBForEFTools.Initialize();
        });
    }

    private static CommandLineBuilder BuildCommandLine()
    {
        var rootCommand = new RootCommand()
        {
            PhotoCommandBuilder.Create<AddHandler, AddParameters>("add", "Add new photos")
                .AddOption(new Option<string[]>("--path"){ AllowMultipleArgumentsPerToken = true, IsRequired = true}).Create(),
            PhotoCommandBuilder.Create<PurgeHandler, PurgeParameters>("purge", "Remove photos marked as deleted").Create(),
            PhotoCommandBuilder.Create<VisionHandler, VisionParameters>("vision", "Runs Azure Vision services over photos").Create(),
        };

        return new CommandLineBuilder(rootCommand);
    }
}

//var result = Parser.Default.ParseArguments<PhotoIntakeOptions, object>(args);

//await result.MapResult(
//    async (PhotoIntakeOptions opt) => await PhotoIntakeOptions.PhotoIntakeProc(opt),
//    errors => Task.FromResult(1));

////var fs = File.OpenRead()
////PhotoIntake.GetMetadata("")


//[Verb("add")]
//public class PhotoIntakeOptions
//{
//    [Value(0, Required = true)]
//    public string FileName { get; set; }

//    public static async Task PhotoIntakeProc(PhotoIntakeOptions opts)
//    {
//        var attr = File.GetAttributes(opts.FileName);

//        var fs = File.OpenRead(opts.FileName);
//        var result = await PhotoIntake.GetMetadata(StoragePath.GetFileName(opts.FileName), fs);
//        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
//        {
//            WriteIndented = true
//        }));
//    }
//}