﻿namespace PhotoPipeline.Framework.Blocks;

public static class BlockNames
{
    public static class Processing
    {
        public const string HashFile = nameof(Blocks.Processing.HashFile);
        public const string ResolveEntity = nameof(Blocks.Processing.ResolveEntity);
        public const string HashPerceptual = nameof(Blocks.Processing.HashPerceptual);
        public const string ReadExif = nameof(Blocks.Processing.ReadExif);

        public const string AzureVision = nameof(Blocks.Processing.AzureVision);
    }

    public static class Utility
    {
        public const string Deduplicate = nameof(Blocks.Utility.Deduplicate);
        public const string WriteFile = nameof(Blocks.Utility.WriteFile);
        public const string RemoveFile = nameof(Blocks.Utility.RemoveFile);
    }
}