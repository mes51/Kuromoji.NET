using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Util
{
    public class ZipResourceResolver : IResourceResolver
    {
        string FilePath { get; }

        public ZipResourceResolver(string filePath)
        {
            FilePath = filePath;
        }

        public Stream Resolve(string resourceName)
        {
            return new ZipReadOnlyStream(FilePath, resourceName);
        }
    }
}
