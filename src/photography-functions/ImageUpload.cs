using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MetadataExtractor;
using System.Linq;

namespace photography_functions
{
    public static class ImageUpload
    {
        [FunctionName("ImageUpload")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            [CosmosDB(databaseName:"photography", collectionName:"photos", ConnectionStringSetting ="ConnectionStrings:Cosmos")]
            IAsyncCollector<dynamic> documentsOut,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Form["filename"];

            var file = req.Form.Files["image"];

            using var fileStream = file.OpenReadStream();

            var reader = ImageMetadataReader.ReadMetadata(fileStream);

            var metadata = reader.SelectMany(m => m.Tags)
                .Where(m => m.HasName)
                .GroupBy(g => g.DirectoryName)
                .ToDictionary(k => k.Key, v => v.Select(t => new
                {
                    t.Type,
                    t.Name,
                    Description = t.Description.Trim()
                }));

            await documentsOut.AddAsync(new
            {
                FileName = name,
                Metadata = metadata
            });
            return new OkObjectResult(new
            {
                FileName = name,
                Metadata = metadata
            });
        }
    }
}
