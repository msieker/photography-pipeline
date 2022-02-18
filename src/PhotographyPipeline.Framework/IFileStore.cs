using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotographyPipeline.Framework
{
    public interface IFileStore
    {
        Task<string> Store(string path, Stream stream);

        Task<string> MakeUrl(string fileId);

        Task<Stream> GetStream(string fileId);
    }
}
