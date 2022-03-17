using MetadataExtractor;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PhotographyPipeline.Framework;

public record BasePhotoMetadata(
    string FileName,
    DateTimeOffset IntakeDate,
    Dictionary<string, string> Hashes,
    Dictionary<string, string> Metadata,
    int Width,
    int Height,
    DateTime? Taken
);

public static class PhotoIntake
{
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
    private static string MakeHash(Stream s)
    {
        using var sha = SHA256.Create();

        var hash = sha.ComputeHash(s);

        return hash
            .Aggregate(new StringBuilder(), (sb, b) => sb.AppendFormat("{0:x2}", b, CultureInfo.InvariantCulture))
            .ToString();
    }

    public static async Task<BasePhotoMetadata> GetMetadata(string fileName, Stream photoStream)
    {
        await using var ms = new MemoryStream();

        await photoStream.CopyToAsync(ms);
        ms.Seek(0, SeekOrigin.Begin);
        var reader = ImageMetadataReader.ReadMetadata(ms);
        ms.Seek(0, SeekOrigin.Begin);

        var metadata = reader.SelectMany(m => m.Tags)
            .Where(m => m.HasName)
            .DistinctBy(k => $"{k.DirectoryName}:{k.Name}")
            .ToDictionary(k => $"{k.DirectoryName}:{k.Name}", v => v.Description?.Trim() ?? "");

        return new BasePhotoMetadata(
            fileName,
            DateTimeOffset.Now,
            new Dictionary<string, string>
            {
                {"sha256",MakeHash(ms) },
            },
            metadata,
            ParseExifSize(metadata["Exif SubIFD:Exif Image Width"]),
            ParseExifSize(metadata["Exif SubIFD:Exif Image Height"]),
            ParseExifDateTime(metadata["Exif IFD0:Date/Time"])
        );
    }
}