using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PhotoPipeline.Common;
using PhotoPipeline.Framework.Storage;

namespace PhotoPipeline.Framework.Blocks.Utility;
internal class WriteFile : IPipelineBlock
{
    private readonly ILogger<WriteFile> _logger;
    private readonly StorageProvider _storageProvider;

    public WriteFile(ILogger<WriteFile> logger, StorageProvider storageProvider)
    {
        _logger = logger;
        _storageProvider = storageProvider;
    }

    public string BlockName => BlockNames.Utility.WriteFile;
    public int BlockVersion => 1;
    public async Task<PipelinePhoto?> Run(PipelinePhoto photo, CancellationToken token)
    {
        if (string.IsNullOrEmpty(photo.OutputPath))
        {
            return (await _storageProvider.Default.Save(photo, token)) ? photo : null;
        }
        else
        {
            var store = _storageProvider.GetFromPath(photo.OutputPath);
            return (await store.Save(photo, token)) ? photo : null;
        }
    }
}