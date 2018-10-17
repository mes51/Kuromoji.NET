using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Buffer
{
    public class BufferEntry
    {
        public List<short> TokenInfo { get; set; } = new List<short>();

        public List<int> Features { get; set; } = new List<int>();

        public List<byte> PosInfo { get; set; } = new List<byte>();

        public short[] TokenInfos { get; set; }

        public int[] FeatureInfos { get; set; }

        public byte[] PosInfos { get; set; }
    }
}
