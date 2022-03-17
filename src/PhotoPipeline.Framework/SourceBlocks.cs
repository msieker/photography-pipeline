//using System.Threading.Tasks.Dataflow;
//using Microsoft.Extensions.Logging;

//namespace PhotoPipeline.Framework;

//public class SourceBlocks
//{
//    private readonly IServiceProvider _serviceProvider;
//    private readonly ILogger<SourceBlocks> _logger;
//    private static readonly string[] KnownExtensions = {".jpg", ".arw"};

//    public SourceBlocks(IServiceProvider serviceProvider, ILogger<SourceBlocks> logger)
//    {
//        _serviceProvider = serviceProvider;
//        _logger = logger;
//    }



//    public TransformManyBlock<string, string> ListDirectory(CancellationToken token=default)
//    => new TransformManyBlock<string, string>()
//    {
//        return new TransformManyBlock<string, string>(path =>
//        {
//            _logger.LogInformation("Reading files from {sourcePath}", path);
//            return Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
//                .Where(p => KnownExtensions.Contains(Path.GetExtension(p), StringComparer.OrdinalIgnoreCase))
//                .ToArray();
//        }, new ExecutionDataflowBlockOptions{CancellationToken = token});
//    }
//}