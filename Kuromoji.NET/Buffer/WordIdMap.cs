using Kuromoji.NET.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Buffer
{
    public class WordIdMap
    {
        readonly int[] Empty = new int[0];

        int[] Indices { get; }

        int[] WordIds { get; }

        public WordIdMap(Stream input)
        {
            var arrays = IntArrayIO.ReadArrays(input, 2);
            Indices = arrays[0];
            WordIds = arrays[1];
        }

        public int[] LookUp(int sourceId)
        {
            var index = Indices[sourceId];

            if (index < 0)
            {
                return Empty;
            }

            var result = new int[WordIds[index]];
            System.Buffer.BlockCopy(WordIds, (index + 1) * sizeof(int), result, 0, result.Length * sizeof(int));
            return result;
        }
    }
}
