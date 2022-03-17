﻿using System.Buffers;
using System.Security.Cryptography;
using System.Text.Json;
using NetTopologySuite.Geometries;
using PhotoPipeline.Database.Entities;

namespace PhotoPipeline.Framework;

public sealed class PipelinePhoto : IDisposable
{
    public PipelinePhoto(string sourcePath)
    {
        SourcePath = sourcePath;
        Exists = false;
        Errored = false;
        FileName = Path.GetFileName(sourcePath);
        Photo = new Photo
        {
            IntakeTake = DateTimeOffset.Now,
            OriginalFileName = FileName,
            OriginalPath = sourcePath
        };
    }

    public string Id { get; set; } = "";
    public string SourcePath { get; set; }
    public string FileName { get; set; }
    public string OutputPath { get; set; } = "";

    public Photo Photo { get; set; }
    public bool Exists { get; set; }
    public bool Errored { get; set; }

    public ReadOnlyMemory<byte>? Memory => _owner?.Memory;

    private IMemoryOwner<byte>? _owner;

    public async Task ReadFile(CancellationToken token=default)
    {
        await using var sourceStream = File.OpenRead(SourcePath);

        _owner = MemoryPool<byte>.Shared.Rent((int) sourceStream.Length);
        await sourceStream.ReadAsync(_owner.Memory, token);

        var hash = SHA256.Create();
        var hashBuffer = new byte[hash.HashSize >> 3];
        if (!hash.TryComputeHash(_owner.Memory.Span, hashBuffer, out _))
        {
            throw new Exception("Couldn't compute a hash for some reason");
        }

        Id = Convert.ToHexString(hashBuffer).ToLowerInvariant();
    }

    public async Task WriteJson(CancellationToken token)
    {
        var obj = new PhotoJson(Photo);

        await using var outFile = File.OpenWrite(OutputPath +".json");
        await JsonSerializer.SerializeAsync(outFile, obj, new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true}, token);
    }

    public void Dispose()
    {
        _owner?.Dispose();
    }
}

public class PhotoJson
{
    public PhotoJson(Photo photo)
    {
        Id = photo.Id;
        OriginalPath = photo.OriginalPath;
        OriginalFileName = photo.OriginalFileName;
        IntakeDate = photo.IntakeTake;

        Width = photo.Width;
        Height = photo.Height;
        Taken = photo.Taken;

        Longitude = photo.Location?.Y;
        Latitude = photo.Location?.X;

        Metadata = photo.Metadata.Select(p => new PhotoMetadataJson(p)).OrderBy(p=>p.Key).ToList();
        Hashes = photo.Hashes.Select(p => new PhotoHashJson(p)).OrderBy(p => p.Type).ToList();
        Steps = photo.PipelineSteps.Select(p => new PipelineStepJson(p)).OrderBy(p => p.Processed).ToList();

    }
    public string Id { get; set; }
    public string OriginalPath { get; set; }
    public string OriginalFileName { get; set; }
    public DateTimeOffset IntakeDate { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime Taken { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public List<PhotoMetadataJson> Metadata { get; set; }
    public List<PhotoHashJson> Hashes { get; set; }
    public List<PipelineStepJson> Steps { get; set; }
}

public class PipelineStepJson
{
    public PipelineStepJson(PhotoPipelineStep step)
    {
        Name = step.StepName;
        Version = step.StepVersion;
        Processed = step.Processed;
    }
    public string Name { get; set; }
    public int Version { get; set; }
    public DateTimeOffset Processed { get; set; }
}

public class PhotoMetadataJson
{
    public PhotoMetadataJson(PhotoMetadata metadata)
    {
        Key = metadata.Key;
        Value = metadata.Value;
        Source = metadata.Source;
    }
    public string Key { get; set; }
    public string Value { get; set; }
    public string Source { get; set; }
}

public class PhotoHashJson
{
    public PhotoHashJson(PhotoHash hash)
    {
        Type = hash.HashType;
        Value = hash.HashValue;
        Source = hash.Source;
    }
    public string Type { get; set; }
    public string Value { get; set; }
    public string Source { get; set; }
}