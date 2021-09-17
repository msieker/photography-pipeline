using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
namespace PhotographyPipeline.Framework
{
    public static class Analyze
    {
        private static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            return new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint,
            };
        }
        public static async Task AzureAnalyze(Stream stream, string visionEndpoint, string visionKey)
        {

            using var image = Image.Load(stream);

            image.Mutate(i => i.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(1920) }));

            using var ms = new MemoryStream();
            image.SaveAsJpeg(ms);
            ms.Seek(0, SeekOrigin.Begin);  
            var visionClient = Authenticate(visionEndpoint, visionKey);
            Console.WriteLine($"Key: {visionKey}, Url: {visionEndpoint}");
            var features = new List<VisualFeatureTypes?> { 
                VisualFeatureTypes.ImageType, VisualFeatureTypes.Faces, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Categories, VisualFeatureTypes.Categories, VisualFeatureTypes.Color,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Description, VisualFeatureTypes.Objects,
                VisualFeatureTypes.Brands
            };

            var analysis = await visionClient.AnalyzeImageInStreamAsync(ms, visualFeatures: features);

            Console.WriteLine(JsonSerializer.Serialize(analysis, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
    }
}
