using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoPipeline.Database.Entities;

namespace PhotographyPipeline.Framework;

internal interface IPhotoPipelineElement
{
    string Name { get; }
    int Version { get; }
    Task<Photo> Process(Photo photo, Stream photoStream);
}