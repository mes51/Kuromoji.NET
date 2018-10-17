using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Util
{
    public interface IResourceResolver
    {
        /// <summary>
        /// Resolve the resource name and return an open input stream to it.
        /// </summary>
        /// <param name="resourceName">resource to resolve</param>
        /// <returns>resolved resource stream</returns>
        /// <exception cref="IOException">if an I/O error occured resolving the resource</exception>
        Stream Resolve(string resourceName);
    }
}
