using Microsoft.Extensions.Logging;

namespace PhotoPipeline.Framework.Blocks.Sources;
public static class ReadFile
{
    public static async Task<PipelinePhoto> Action(string path, ILogger logger, CancellationToken token = default)
    {
        logger.LogInformation("Reading file {photoPath}", path);
        var photo = new PipelinePhoto(path);
        await photo.ReadFile(token);

        return photo;
    }
}
