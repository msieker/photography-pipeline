using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using PhotoPipeline.Common;
using PhotoPipeline.Database.Entities;
using PhotoPipeline.Framework.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PhotoPipeline.Framework.Blocks.Processing;
internal class AzureVision : IPipelineBlock
{
    private static SemaphoreSlim LimitSemaphore = new(8);
    private readonly ComputerVisionClient _visionClient;
    private readonly ILogger<AzureVision> _logger;
    public AzureVision(StorageProvider storageProvider, PhotoPipelineConfig config, ILogger<AzureVision> logger)
    {
        _storageProvider = storageProvider;
        _logger = logger;

        if (string.IsNullOrEmpty(config.AzureVision.Key)) throw new InvalidOperationException("AzureVision.Key is not set in the configuration");
        if (string.IsNullOrEmpty(config.AzureVision.Endpoint)) throw new InvalidOperationException("AzureVision.Endpoint is not set in the configuration");

        _visionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(config.AzureVision.Key)) {Endpoint = config.AzureVision.Endpoint};
    }

    public string BlockName => BlockNames.Processing.AzureVision;
    public int BlockVersion => 1;

    private const int MaxSize = 1920;

    private readonly StorageProvider _storageProvider;

    public async Task<PipelinePhoto?> Run(PipelinePhoto photo, CancellationToken token = default)
    {
        if (photo.Photo.PipelineSteps.Exists(s => s.StepName == BlockName && s.StepVersion == BlockVersion))
        {
            _logger.LogInformation("Photo {photoPath} {hash} already analyzed", photo.SourcePath,photo.Id);
            return null;
        }

        if (!Path.GetExtension(photo.Photo.OriginalFileName).Equals(".jpg", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var storage = _storageProvider.GetFromPath(photo.OutputPath);
        await storage.Read(photo, token);
        if (photo.Memory == null)
        {
            _logger.LogWarning("No memory for image even though it was just read");
            return null;
        }
        Stream imgStream;
        if (photo.Photo.Width > 1920 || photo.Photo.Height > 1920)
        {
            _logger.LogInformation("Resizing photo {photoPath} {hash}", photo.SourcePath, photo.Id);
            var img = Image.Load(photo.Memory.Value.Span);
            img.Mutate(i => i.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(1920) }));
            imgStream = new MemoryStream();
            await img.SaveAsJpegAsync(imgStream, token);
            imgStream.Seek(0, SeekOrigin.Begin);
        }
        else
        {
            imgStream = photo.Memory.Value.AsStream();
        }

        var features = new List<VisualFeatureTypes?> {
            VisualFeatureTypes.ImageType, VisualFeatureTypes.Faces, VisualFeatureTypes.Adult,
            VisualFeatureTypes.Categories, VisualFeatureTypes.Categories, VisualFeatureTypes.Color,
            VisualFeatureTypes.Tags, VisualFeatureTypes.Description, VisualFeatureTypes.Objects,
            VisualFeatureTypes.Brands
        };
        var details = new List<Details?>{
            Details.Landmarks,
            Details.Celebrities
        };
        await LimitSemaphore.WaitAsync(token);
        try
        {
            _logger.LogInformation("Analyzing {photoPath} {hash}", photo.SourcePath, photo.Id);
            var analysis = await _visionClient.AnalyzeImageInStreamAsync(imgStream, features, details, cancellationToken: token);

            var metadata = photo.Photo.Metadata.FirstOrDefault(m => m.Key == "azurevision::analysis");
            if (metadata == null)
            {
                metadata = new PhotoMetadata {Key = "azurevision::analysis", Source = BlockNames.Processing.AzureVision, Photo = photo.Photo, PhotoId = photo.Photo.Id};
                photo.Photo.Metadata.Add(metadata);
            }

            metadata.Value = JsonSerializer.Serialize(analysis);
            return photo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error calling Analyze Image");
            throw;
            //return null;
        }
        finally
        {
            LimitSemaphore.Release();
            await imgStream.DisposeAsync();
        }


    }
}
