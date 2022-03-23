using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;

namespace PhotoPipeline.Framework.Blocks.Sources;
public static class ListDirectory
{
    private static readonly string[] KnownExtensions = { ".jpg", ".arw" };

    public static IEnumerable<string> Action(string path, ILogger logger, CancellationToken token = default)
    {
        logger.LogInformation("Reading files from {sourcePath}", path);
        
        return Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(p => KnownExtensions.Contains(Path.GetExtension(p), StringComparer.OrdinalIgnoreCase))
            .ToArray();
    }
}
