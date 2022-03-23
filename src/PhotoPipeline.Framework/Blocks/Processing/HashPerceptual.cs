using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using PhotoPipeline.Database.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PhotoPipeline.Framework.Blocks.Processing;
public class HashPerceptual : IPipelineBlock
{
    private readonly ILogger<HashPerceptual> _logger;
    private static readonly string[] PerceptualExtensions = {".jpg", ".png"};

    public HashPerceptual(ILogger<HashPerceptual> logger)
    {
        _logger = logger;
    }

    public string BlockName => BlockNames.Processing.HashPerceptual;
    public int BlockVersion => 1;

    public Task<PipelinePhoto?> Run(PipelinePhoto photo, CancellationToken token)
    {
        if (photo.Memory == null)
        {
            _logger.LogWarning("Got a photo with no memory");
            return Task.FromResult(photo)!;
        }
        if (!PerceptualExtensions.Contains(Path.GetExtension(photo.FileName), StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(photo)!;
        }
        _logger.LogDebug("Perceptual Hashing file {photoPath}", photo.SourcePath);

        var hashDict = new Dictionary<string, IImageHash>()
        {
            {"average", new AverageHash()},
            {"difference", new DifferenceHash()},
            {"perceptual", new PerceptualHash()},
        };

        try
        {
            using var ms = photo.Memory.Value.AsStream();
            using var image = Image.Load<Rgba32>(ms);
            foreach (var (name, alg) in hashDict)
            {
                var hashed = alg.Hash(image);
                var hashString = Convert.ToHexString(BitConverter.GetBytes(hashed));
                photo.Photo.Hashes.Add(new PhotoHash {HashType = name, HashValue = hashString, Source = BlockName});
            }
        }
        catch (NotSupportedException e)
        {
            _logger.LogWarning(e, "Problem processing {imageFile}", photo.SourcePath);
        }
        catch (InvalidImageContentException iice)
        {
            _logger.LogError(iice, "Problem processing {imageFile}, possibly corrupt. Skipping.", photo.SourcePath);
            return Task.FromResult((PipelinePhoto?) null);
        }

        return Task.FromResult(photo)!;
    }
}
