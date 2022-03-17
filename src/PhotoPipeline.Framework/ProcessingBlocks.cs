//using System.Globalization;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks.Dataflow;
//using CoenM.ImageHash;
//using CoenM.ImageHash.HashAlgorithms;
//using MetadataExtractor;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Microsoft.Toolkit.HighPerformance;
//using PhotoPipeline.Common;
//using PhotoPipeline.Database;
//using PhotoPipeline.Database.Entities;

//namespace PhotoPipeline.Framework;

//public class ProcessingBlocks
//{
//    private readonly ILogger<ProcessingBlocks> _logger;
//    private readonly IServiceProvider _serviceProvider;
//    private readonly PhotoPipelineConfig _config;

//    public ProcessingBlocks(ILogger<ProcessingBlocks> logger, IServiceProvider serviceProvider, PhotoPipelineConfig config)
//    {
//        _logger = logger;
//        _serviceProvider = serviceProvider;
//        _config = config;
        
//    }

//    private ExecutionDataflowBlockOptions MakeOptions(CancellationToken token) => new()
//    {
//        MaxDegreeOfParallelism = _config.MaxParallelism,
//        CancellationToken = token
//    };

//    public TransformBlock<string, PipelinePhoto> ReadFile(CancellationToken token=default)
//    {
//        return new TransformBlock<string, PipelinePhoto>(async path =>
//        {
//            _logger.LogInformation("Reading file {photoPath}", path);
//            var photo = new PipelinePhoto(path);
//            await photo.ReadFile(token);

//            return photo;
//        }, MakeOptions(token));
//    }

//    public TransformBlock<PipelinePhoto, PipelinePhoto> ResolveEntity(CancellationToken token = default)
//    {
//        return new TransformBlock<PipelinePhoto, PipelinePhoto>(async photo =>
//        {
//            await using var scope = _serviceProvider.CreateAsyncScope();
//            var context = scope.ServiceProvider.GetRequiredService<PhotoDbContext>();

//            var entity = await context.Photos
//                .Include(p => p.Hashes)
//                .Include(p => p.Metadata)
//                .Include(p => p.PipelineSteps)
//                .FirstOrDefaultAsync(p => p.Id == photo.Id, token);

//            if (entity == null)
//            {
//                _logger.LogInformation("Photo {photoPath} with id {photoId} is new", photo.SourcePath, photo.Id);
//                photo.Photo.Id = photo.Id;
//                photo.Photo.OriginalFileName = photo.FileName;
//            }
//            else
//            {
//                _logger.LogInformation("Photo {photoPath} with id {photoId} already exists", photo.SourcePath, photo.Id);
//                photo.Exists = true;
//            }
//            return photo;
//        }, MakeOptions(token));
//    }

//    public TransformBlock<PipelinePhoto, PipelinePhoto> FileHashes(CancellationToken token =default)
//    {
//        return new TransformBlock<PipelinePhoto, PipelinePhoto>(photo =>
//        {
//            if (photo.Memory == null)
//            {
//                _logger.LogWarning("Got a photo with no memory");
//                return photo;
//            }
//            _logger.LogInformation("Hashing file {photoPath}", photo.SourcePath);
//            var hashDict = new Dictionary<string, HashAlgorithm>()
//            {
//                {"sha1", SHA1.Create()},
//                {"sha256", SHA256.Create()},
//                {"sha512", SHA512.Create()},
//            };
            
//            foreach (var (name, alg) in hashDict)
//            {
//                var hashBuffer = new byte[alg.HashSize >> 3];
//                if (!alg.TryComputeHash(photo.Memory.Value.Span, hashBuffer, out _))
//                {
//                    throw new Exception("Couldn't compute a hash for some reason");
//                }
//                var hashString = Convert.ToHexString(hashBuffer).ToLowerInvariant();

//                photo.Photo.Hashes.Add(new PhotoHash { HashType = name, HashValue = hashString });
//            }

//            return photo;
//        }, MakeOptions(token));
//    }

//    public TransformBlock<PipelinePhoto, PipelinePhoto> PerceptualHashes(CancellationToken token=default)
//    {
//        var hashable = new[] { ".jpg", ".png" };

//        return new TransformBlock<PipelinePhoto, PipelinePhoto>(photo =>
//        {
//            if (photo.Memory == null)
//            {
//                _logger.LogWarning("Got a photo with no memory");
//                return photo;
//            }
//            if (!hashable.Contains(Path.GetExtension(photo.FileName), StringComparer.OrdinalIgnoreCase))
//            {
//                return photo;
//            }
//            _logger.LogInformation("Perceptual Hashing file {photoPath}", photo.SourcePath);

//            var hashDict = new Dictionary<string, IImageHash>()
//            {
//                {"average", new AverageHash()},
//                {"difference", new DifferenceHash()},
//                {"perceptual", new PerceptualHash()},
//            };

//            foreach (var (name, alg) in hashDict)
//            {
//                using var ms = photo.Memory.Value.AsStream();
//                var hashed = alg.Hash(ms);

//                var hashString = Convert.ToHexString(BitConverter.GetBytes(hashed));

//                photo.Photo.Hashes.Add(new PhotoHash { HashType = name, HashValue = hashString });
//            }

//            return photo;
//        }, MakeOptions(token));
//    }

//    public TransformBlock<PipelinePhoto, PipelinePhoto> ExtractExif(CancellationToken token=default)
//    {
//        var extractable = new[] { ".jpg", ".arw" };

//        static int ParseExifSize(string? size)
//        {
//            if (size == null) return 0;
//            var parts = size.Split(' ');
//            return int.Parse(parts[0], CultureInfo.InvariantCulture);
//        }

//        static DateTime? ParseExifDateTime(string? dateTime)
//        {
//            if (dateTime == null) return null;
//            return DateTime.ParseExact(dateTime, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
//        }

//        return new TransformBlock<PipelinePhoto, PipelinePhoto>(photo =>
//        {
//            if (photo.Memory == null)
//            {
//                _logger.LogWarning("Got a photo with no memory");
//                return photo;
//            }

//            if (!extractable.Contains(Path.GetExtension(photo.FileName), StringComparer.OrdinalIgnoreCase))
//            {
//                return photo;
//            }
//            _logger.LogInformation("Extracting exif of {photoPath}", photo.SourcePath);
//            using var ms = photo.Memory.Value.AsStream();
//            var reader = ImageMetadataReader.ReadMetadata(ms);
            
//            var metadata = reader.SelectMany(m => m.Tags)
//                .Where(m => m.HasName)
//                .DistinctBy(k => $"{k.DirectoryName}:{k.Name}")
//                .ToDictionary(k => $"{k.DirectoryName}:{k.Name}", v => v.Description?.Trim() ?? "");


//            foreach (var (k, v) in metadata)
//            {
//                photo.Photo.Metadata.Add(new PhotoMetadata
//                {
//                    Key = k,
//                    Value = v
//                });
//            }
//            photo.Photo.Width = ParseExifSize(metadata["Exif SubIFD:Exif Image Width"]);
//            photo.Photo.Height = ParseExifSize(metadata["Exif SubIFD:Exif Image Height"]);
//            photo.Photo.Taken = ParseExifDateTime(metadata["Exif IFD0:Date/Time"]) ?? DateTime.Parse(metadata["File:File Modified Date"]); 
//            return photo;
//        }, MakeOptions(token));
//    }
//}