using System.Globalization;
using System.Text.RegularExpressions;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using NetTopologySuite.Geometries;
using PhotoPipeline.Database.Entities;

namespace PhotoPipeline.Framework.Blocks.Processing;
public class ReadExif : IPipelineBlock
{
    private static readonly string[] ExifExtensions = new[] {".jpg", ".arw"};
    private static readonly Regex SanitizeRegex = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);
    private readonly ILogger<ReadExif> _logger;

    public ReadExif(ILogger<ReadExif> logger)
    {
        _logger = logger;
    }

    private static string Sanitize(string s) => SanitizeRegex.Replace(s.ToLowerInvariant(), "_");

    private static int ParseExifSize(string? size)
    {
        if (size == null) return 0;
        var parts = size.Split(' ');
        return int.Parse(parts[0], CultureInfo.InvariantCulture);
    }

    private static DateTime? ParseExifDateTime(string? dateTime)
    {
        if (dateTime == null) return null;
        return DateTime.ParseExact(dateTime, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private static int? CoalesceTagInt32(MetadataExtractor.Directory? dir, params int[] tags)
    {
        if(dir == null) return null;

        foreach (var t in tags)
        {
            if (dir.ContainsTag(t))
            {
                return dir.GetInt32(t);
            }
        }

        return null;
    }

    public string BlockName => BlockNames.Processing.ReadExif;
    public int BlockVersion => 1;

    public async Task<PipelinePhoto?> Run(PipelinePhoto photo, CancellationToken token)
    {
        if (photo.Memory == null)
        {
            _logger.LogWarning("Got a photo with no memory");
            return photo;
        }

        if (!ExifExtensions.Contains(Path.GetExtension(photo.FileName), StringComparer.OrdinalIgnoreCase))
        {
            return photo;
        }
        _logger.LogInformation("Extracting exif of {photoPath}", photo.SourcePath);
        await using var ms = photo.Memory.Value.AsStream();
        var reader = ImageMetadataReader.ReadMetadata(ms);

        var metadata = reader.SelectMany(m => m.Tags)
            .Where(m => m.HasName)
            .Select(m => new { DirectoryName = Sanitize(m.DirectoryName), Name = Sanitize(m.Name), Value = m.Description?.Trim() ?? "" })
            .Where(m => !string.IsNullOrEmpty(m.Value))
            .DistinctBy(k => $"{k.DirectoryName}::{k.Name}")
            .ToDictionary(k => $"{k.DirectoryName}::{k.Name}", v => v.Value);


        foreach (var (k, v) in metadata)
        {
            photo.Photo.Metadata.Add(new PhotoMetadata
            {
                Key = k,
                Value = v,
                Source = BlockName
            });
        }

        var ifd0 = reader.OfType<ExifIfd0Directory>().FirstOrDefault();
        photo.Photo.Width = CoalesceTagInt32(ifd0, ExifDirectoryBase.TagImageWidth, ExifDirectoryBase.TagExifImageWidth) ?? 0;
        photo.Photo.Height = CoalesceTagInt32(ifd0, ExifDirectoryBase.TagImageHeight, ExifDirectoryBase.TagExifImageHeight) ?? 0;
        photo.Photo.Taken = ifd0?.GetDateTime(ExifDirectoryBase.TagDateTime) ?? reader.OfType<FileMetadataDirectory>().First().GetDateTime(FileMetadataDirectory.TagFileModifiedDate);

        var gps = reader.OfType<GpsDirectory>().FirstOrDefault()?.GetGeoLocation();
        if (gps != null)
        {
            photo.Photo.Location = new Point(gps.Latitude, gps.Longitude);
        }
        return photo;
    }
}
