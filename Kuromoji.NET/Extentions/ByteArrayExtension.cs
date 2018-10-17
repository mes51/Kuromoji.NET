using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Extentions
{
    public static class ByteArrayExtension
    {
        public static int ReadInt32(this byte[] data, int index)
        {
            unchecked
            {
                return (data[index] << 24) | (data[index + 1] << 16) | (data[index + 2] << 8) | data[index + 3];
            };
        }

        public static short ReadInt16(this byte[] data, int index)
        {
            unchecked
            {
                return (short)((data[index] << 8) | data[index + 1]);
            }
        }

        public static void WriteInt32(this byte[] data, int index, int value)
        {
            data[index] = (byte)((value >> 24) & 0xFF);
            data[index + 1] = (byte)((value >> 16) & 0xFF);
            data[index + 2] = (byte)((value >> 8) & 0xFF);
            data[index + 3] = (byte)(value & 0xFF);
        }

        public static void WriteInt16(this byte[] data, int index, short value)
        {
            data[index] = (byte)((value >> 8) & 0xFF);
            data[index + 1] = (byte)(value & 0xFF);
        }
    }
}
