using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using PhotoPipeline.Database.Entities;

namespace PhotoPipeline.Framework.Blocks.Processing;
public class HashFile : IPipelineBlock
{
    private readonly ILogger<HashFile> _logger;

    public HashFile(ILogger<HashFile> logger)
    {
        _logger = logger;
    }

    public string BlockName => BlockNames.Processing.HashFile;
    public int BlockVersion => 1;

    public Task<PipelinePhoto?> Run(PipelinePhoto photo, CancellationToken token)
    {
        if (photo.Memory == null)
        {
            _logger.LogWarning("Got a photo with no memory");
            return Task.FromResult(photo)!;
        }
        _logger.LogDebug("Hashing file {photoPath}", photo.SourcePath);
        var hashDict = new Dictionary<string, HashAlgorithm>()
        {
            {"sha1", SHA1.Create()},
            {"sha256", SHA256.Create()},
            {"sha512", SHA512.Create()},
        };

        foreach (var (name, alg) in hashDict)
        {
            var hashBuffer = new byte[alg.HashSize >> 3];
            if (!alg.TryComputeHash(photo.Memory.Value.Span, hashBuffer, out _))
            {
                throw new Exception("Couldn't compute a hash for some reason");
            }
            var hashString = Convert.ToHexString(hashBuffer).ToLowerInvariant();

            photo.Photo.Hashes.Add(new PhotoHash { HashType = name, HashValue = hashString, Source = BlockName});
        }

        return Task.FromResult(photo)!;
    }
}
