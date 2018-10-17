using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Extentions;

namespace Kuromoji.NET.IO
{
    public static class ByteBufferIO
    {
        public static byte[] Read(Stream input)
        {
            var result = new byte[input.ReadInt32()];
            input.Read(result, 0, result.Length);
            return result;
        }

        public static void Write(Stream output, byte[] data)
        {
            output.Write(data.Length);
            output.Write(data, 0, data.Length);
        }
    }
}
