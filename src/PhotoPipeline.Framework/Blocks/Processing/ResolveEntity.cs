using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoPipeline.Database;

namespace PhotoPipeline.Framework.Blocks.Processing;

public class ResolveEntity : IPipelineBlock
{
    private readonly ILogger<ResolveEntity> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ResolveEntity(ILogger<ResolveEntity> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public string BlockName => BlockNames.Processing.ResolveEntity;
    public int BlockVersion => 1;
    public async Task<PipelinePhoto?> Run(PipelinePhoto photo, CancellationToken token)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<PhotoDbContext>();

        var entity = await context.Photos
            .Include(p => p.Hashes)
            .Include(p => p.Metadata)
            .Include(p => p.PipelineSteps)
            .FirstOrDefaultAsync(p => p.Id == photo.Id, token);

        if (entity == null)
        {
            _logger.LogDebug("Photo {photoPath} with id {photoId} is new", photo.SourcePath, photo.Id);
            photo.Photo.Id = photo.Id;
            photo.Photo.OriginalFileName = photo.FileName;
        }
        else
        {
            _logger.LogDebug("Photo {photoPath} with id {photoId} already exists", photo.SourcePath, photo.Id);
            return null;
        }
        return photo;
    }
}
