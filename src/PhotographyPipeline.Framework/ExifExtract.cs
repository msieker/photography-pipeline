using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoPipeline.Database.Entities;

namespace PhotographyPipeline.Framework
{
    internal class ExifExtract : IPhotoPipelineElement
    {
        public string Name => "ExifExtract";
        public int Version => 1;

        public Task<Photo> Process(Photo photo, Stream photoStream)
        {
            throw new NotImplementedException();
        }
    }
}
