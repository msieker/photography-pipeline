using Microsoft.Extensions.Logging;
using PhotoPipeline.Common;

namespace PhotoPipeline.Framework.Storage;

public class LocalFilesystemStorageProvider : IStorageProvider
{
    private readonly ILogger<LocalFilesystemStorageProvider> _logger;
    public string Name => "local";

    private readonly LocalStorageConfig _config;
    public LocalFilesystemStorageProvider(PhotoPipelineConfig config, ILogger<LocalFilesystemStorageProvider> logger)
    {
        _logger = logger;
        _config = config.Storage.Local ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<bool> Save(PipelinePhoto photo, CancellationToken token = default)
    {
        if (photo.SourcePath != photo.OutputPath)
        {
            if (photo.Memory == null) return false;
            var basePath = Path.Combine(_config.StoragePath, photo.Photo.Taken.ToString("yyyy"), photo.Photo.Taken.ToString("MM"), photo.Photo.Taken.ToString("dd"));

            Directory.CreateDirectory(basePath);

            var written = false;
            var filePath = Path.Combine(basePath, photo.FileName);
            var hashSuffixSize = 1;
            while (!written)
            {
                try
                {
                    await using var outFile = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true);
                    await outFile.WriteAsync(photo.Memory.Value, token);
                    written = true;
                    photo.Photo.StoredPath = filePath;
                    photo.OutputPath = filePath;
                }
                catch (IOException)
                {
                    var newFileName = string.Concat(Path.GetFileNameWithoutExtension(photo.FileName), "-", photo.Id.AsSpan(0, hashSuffixSize), Path.GetExtension(photo.FileName));
                    filePath = Path.Join(basePath, newFileName);
                    written = false;
                    hashSuffixSize++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error writing data for {fileName} ({hash}) to path {path}", photo.FileName, photo.Id, basePath);
                    throw;
                }

                if (hashSuffixSize > photo.Id.Length)
                {
                    _logger.LogError("Ran out of hash bytes when trying to copy {fileName} ({hash}) to path {path}", photo.FileName, photo.Id, basePath);
                    return false;
                }
            }
        }

        var sidecarPath = photo.OutputPath + ".json";
        await using var sidecarStream = File.OpenWrite(sidecarPath);
        await photo.WriteJson(sidecarStream, token);

        return true;
    }

    public Task<bool> Delete(PipelinePhoto photo, CancellationToken token = default)
    {
        var paths = new[] { photo.OutputPath, photo.OutputPath + ".json" };


        foreach (var p in paths)
        {
            try
            {
                switch (_config.DeleteBehavior)
                {
                    case LocalStorageDeleteBehavior.CopyToTrash:
                    {
                        var relPath = Path.GetRelativePath(_config.StoragePath, photo.OutputPath);
                        var newPath = Path.Combine(_config.TrashPath, relPath);
                        _logger.LogDebug("Moving {sourcePath} to trash {trashPath}", p, newPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                        File.Move(p, newPath, true);
                        break;
                    }
                    case LocalStorageDeleteBehavior.Delete:
                        File.Delete(p);
                        break;
                    case LocalStorageDeleteBehavior.Nothing:
                    case LocalStorageDeleteBehavior.Unknown:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Problem removing file {path}", p);
                return Task.FromResult(false);
            }
        }
        return Task.FromResult(true);
    }

    public async Task<bool> Read(PipelinePhoto photo, CancellationToken token)
    {
        if (photo.Memory != null) return true;

        await using var sourceStream = File.OpenRead(photo.SourcePath);
        await photo.ReadFromStream(sourceStream, token);

        return true;
    }

    public bool CanHandle(string path) => path.StartsWith(_config.StoragePath);
}