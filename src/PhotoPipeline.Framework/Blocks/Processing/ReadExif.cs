using System.Globalization;
using System.Text.RegularExpressions;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;
using MetadataExtractor.Formats.Jpeg;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PhotoPipeline.Database.Entities;
using Directory = MetadataExtractor.Directory;

namespace PhotoPipeline.Framework.Blocks.Processing;
public class ReadExif : IPipelineBlock
{
    private static readonly string[] ExifExtensions = new[] { ".jpg", ".arw" };
    private static readonly Regex SanitizeRegex = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);
    private readonly ILogger<ReadExif> _logger;
    private static readonly GeometryFactory GeometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
    public ReadExif(ILogger<ReadExif> logger)
    {
        _logger = logger;
    }

    private static string Sanitize(string s) => SanitizeRegex.Replace(s.ToLowerInvariant(), "_");

    private DateTime? ParseExifDateTime(string? dateTime)
    {
        try
        {
            if (dateTime == null) return null;
            var parts = dateTime.Split(' ');
            if (parts.Length != 2) return null;
            var dateParts = parts[0].Split(":").Select(s => int.Parse(s)).ToList();
            if (dateParts.Count != 3) return null;
            var timeParts = parts[1].Split(":").Select(s => int.Parse(s)).ToList();
            if (timeParts.Count != 3) return null;

            return new DateTime(dateParts[0], dateParts[1], dateParts[2], timeParts[0] % 24, timeParts[1] % 60, timeParts[2] % 60);
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public static T? CoalesceTag<T>(IEnumerable<MetadataExtractor.Directory> dirs, Func<Directory, int, T> selector, params int[] tags) where T : struct
    {
        foreach (var d in dirs)
        {
            foreach (var t in tags)
            {
                if (d.ContainsTag(t))
                {
                    return selector(d, t);
                }
            }
        }

        return null;
    }


    public static string? CoalesceTagString(IEnumerable<MetadataExtractor.Directory> dirs, Func<Directory, int, string> selector, params int[] tags)
    {
        foreach (var d in dirs)
        {
            foreach (var t in tags)
            {
                if (d.ContainsTag(t))
                {
                    return selector(d, t);
                }
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
        _logger.LogDebug("Extracting exif of {photoPath}", photo.SourcePath);
        await using var ms = photo.Memory.Value.AsStream();
        var reader = ImageMetadataReader.ReadMetadata(ms);

        var metadata = reader.SelectMany(m => m.Tags)
            .Where(m => m.HasName)
            .Select(m => new { DirectoryName = Sanitize(m.DirectoryName), Name = Sanitize(m.Name), Value = m.Description?.Trim() ?? "" })
            .Where(m => !string.IsNullOrEmpty(m.Value) && m.Value.Length < 400)
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

        var ifd = reader.OfType<ExifIfd0Directory>().Cast<ExifDirectoryBase>().Concat(reader.OfType<ExifSubIfdDirectory>()).ToList();
        var jpg = reader.OfType<JpegDirectory>().ToList();
        photo.Photo.Width = CoalesceTag(ifd, (d, t) => d.GetInt32(t), ExifDirectoryBase.TagImageWidth, ExifDirectoryBase.TagExifImageWidth) ?? CoalesceTag(jpg, (d, t) => d.GetInt32(t), JpegDirectory.TagImageWidth) ?? 0;
        photo.Photo.Height = CoalesceTag(ifd, (d, t) => d.GetInt32(t), ExifDirectoryBase.TagImageHeight, ExifDirectoryBase.TagExifImageHeight) ?? CoalesceTag(jpg, (d, t) => d.GetInt32(t), JpegDirectory.TagImageHeight) ?? 0;

        try
        {
            photo.Photo.Taken = CoalesceTag(ifd, (d, t) => d.GetDateTime(t), ExifDirectoryBase.TagDateTime)
                                ?? photo.FileModifiedDate;
        }
        catch (MetadataException e)
        {
            if (e.Message.Contains("306"))
            {
                var value = CoalesceTagString(ifd, (d, t) => (d.GetString(t) ?? ""), ExifDirectoryBase.TagDateTime);
                _logger.LogWarning("Got a funky DateTime on {photoPath} of {value}", photo.SourcePath, value);
                photo.Photo.Taken = ParseExifDateTime(value) ?? photo.FileModifiedDate;
            }
            else
            {
                throw;
            }
        }


        var gps = reader.OfType<GpsDirectory>().FirstOrDefault()?.GetGeoLocation();
        if (gps != null)
        {
            photo.Photo.Location = GeometryFactory.CreatePoint(new Coordinate(gps.Longitude, gps.Latitude));
        }
        return photo;
    }
}
