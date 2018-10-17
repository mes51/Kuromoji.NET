using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Extentions
{
    public static class StreamExtension
    {
        public static bool ReadBool(this Stream stream)
        {
            return stream.ReadByte() > 0;
        }

        public static int ReadInt32(this Stream stream)
        {
            return stream.ReadByte() << 24 |
                stream.ReadByte() << 16 |
                stream.ReadByte() << 8 |
                stream.ReadByte();
        }

        public static short ReadInt16(this Stream stream)
        {
            return unchecked((short)(stream.ReadByte() << 8 | stream.ReadByte()));
        }

        public static void ReadIntArray(this Stream stream, int[] result, int count = -1)
        {
            if (count < 0)
            {
                count = result.Length;
            }
            byte[] tmp = new byte[count * sizeof(int)];
            stream.Read(tmp, 0, tmp.Length);
            for (int i = 0, pos = 0; i < count; i++, pos += sizeof(int))
            {
                result[i] = tmp[pos] << 24 |
                    tmp[pos + 1] << 16 |
                    tmp[pos + 2] << 8 |
                    tmp[pos + 3];
            }
        }

        public static void ReadIntList(this Stream stream, List<int> result, int count = -1)
        {
            if (count < 0)
            {
                count = result.Count;
            }
            byte[] tmp = new byte[count * sizeof(int)];
            stream.Read(tmp, 0, tmp.Length);
            for (int i = 0, pos = 0; i < count; i++, pos += sizeof(int))
            {
                result[i] = tmp[pos] << 24 |
                    tmp[pos + 1] << 16 |
                    tmp[pos + 2] << 8 |
                    tmp[pos + 3];
            }
        }

        public static void ReadShortArray(this Stream stream, short[] result, int count = -1)
        {
            if (count < 0)
            {
                count = result.Length;
            }
            byte[] tmp = new byte[count * sizeof(short)];
            stream.Read(tmp, 0, tmp.Length);
            for (int i = 0, pos = 0; i < count; i++, pos += sizeof(short))
            {
                result[i] = unchecked((short)(tmp[pos] << 8 | tmp[pos + 1]));
            }
        }

        public static void ReadShortList(this Stream stream, List<short> result, int count = -1)
        {
            if (count < 0)
            {
                count = result.Count;
            }
            byte[] tmp = new byte[count * sizeof(short)];
            stream.Read(tmp, 0, tmp.Length);
            for (int i = 0, pos = 0; i < count; i++, pos += sizeof(short))
            {
                result[i] = unchecked((short)(tmp[pos] << 8 | tmp[pos + 1]));
            }
        }

        public static void ReadCharArray(this Stream stream, char[] result, int count = -1)
        {
            if (count < 0)
            {
                count = result.Length;
            }
            byte[] tmp = new byte[count * sizeof(char)];
            stream.Read(tmp, 0, tmp.Length);
            for (int i = 0, pos = 0; i < count; i++, pos += sizeof(char))
            {
                result[i] = unchecked((char)(tmp[pos] << 8 | tmp[pos + 1]));
            }
        }

        public static void ReadCharList(this Stream stream, List<char> result, int count = -1)
        {
            if (count < 0)
            {
                count = result.Count;
            }
            byte[] tmp = new byte[count * sizeof(char)];
            stream.Read(tmp, 0, tmp.Length);
            for (int i = 0, pos = 0; i < count; i++, pos += sizeof(char))
            {
                result[i] = unchecked((char)(tmp[pos] << 8 | tmp[pos + 1]));
            }
        }

        public static void Write(this Stream stream, bool value)
        {
            if (value)
            {
                stream.WriteByte(1);
            }
            else
            {
                stream.WriteByte(0);
            }
        }

        public static void Write(this Stream stream, int value)
        {
            stream.WriteByte((byte)((value >> 24) & 0xFF));
            stream.WriteByte((byte)((value >> 16) & 0xFF));
            stream.WriteByte((byte)((value >> 8) & 0xFF));
            stream.WriteByte((byte)(value & 0xFF));
        }

        public static void Write(this Stream stream, short value)
        {
            stream.WriteByte((byte)((value >> 8) & 0xFF));
            stream.WriteByte((byte)(value & 0xFF));
        }

        public static void Write(this Stream stream, int[] value, int count = -1)
        {
            if (count < 0)
            {
                count = value.Length;
            }
            byte[] tmp = new byte[count * sizeof(int)];
            for (int i = 0, pos = 0; i < count; i++, pos += sizeof(int))
            {
                tmp[pos] = (byte)((value[i] >> 24) & 0xFF);
                tmp[pos + 1] = (byte)((value[i] >> 16) & 0xFF);
                tmp[pos + 2] = (byte)((value[i] >> 8) & 0xFF);
                tmp[pos + 3] = (byte)(value[i] & 0xFF);
            }
            stream.Write(tmp, 0, tmp.Length);
        }

        public static void Write(this Stream stream, List<int> value, int count = -1)
        {
            if (count < 0)
            {
                count = value.Count;
            }
            byte[] tmp = new byte[count * sizeof(int)];
            for (int i = 0, pos = 0; i < count; i++, pos += sizeof(int))
            {
                tmp[pos] = (byte)((value[i] >> 24) & 0xFF);
                tmp[pos + 1] = (byte)((value[i] >> 16) & 0xFF);
                tmp[pos + 2] = (byte)((value[i] >> 8) & 0xFF);
                tmp[pos + 3] = (byte)(value[i] & 0xFF);
            }
            stream.Write(tmp, 0, tmp.Length);
        }

        public static void Write(this Stream stream, short[] value, int count = -1)
        {
            if (count < 0)
            {
                count = value.Length;
            }
            byte[] tmp = new byte[count * sizeof(short)];
            for (int i = 0, pos = 0; i < count; i++, pos += sizeof(short))
            {
                tmp[pos] = (byte)((value[i] >> 8) & 0xFF);
                tmp[pos + 1] = (byte)(value[i] & 0xFF);
            }
            stream.Write(tmp, 0, tmp.Length);
        }

        public static void Write(this Stream stream, List<short> value, int count = -1)
        {
            if (count < 0)
            {
                count = value.Count;
            }
            byte[] tmp = new byte[count * sizeof(short)];
            for (int i = 0, pos = 0; i < count; i++, pos += sizeof(short))
            {
                tmp[pos] = (byte)((value[i] >> 8) & 0xFF);
                tmp[pos + 1] = (byte)(value[i] & 0xFF);
            }
            stream.Write(tmp, 0, tmp.Length);
        }

        public static void Write(this Stream stream, char[] value, int count = -1)
        {
            if (count < 0)
            {
                count = value.Length;
            }
            byte[] tmp = new byte[count * sizeof(short)];
            for (int i = 0, pos = 0; i < count; i++, pos += sizeof(short))
            {
                tmp[pos] = (byte)((value[i] >> 8) & 0xFF);
                tmp[pos + 1] = (byte)(value[i] & 0xFF);
            }
            stream.Write(tmp, 0, tmp.Length);
        }

        public static void Write(this Stream stream, List<char> value, int count = -1)
        {
            if (count < 0)
            {
                count = value.Count;
            }
            byte[] tmp = new byte[count * sizeof(short)];
            for (int i = 0, pos = 0; i < count; i++, pos += sizeof(short))
            {
                tmp[pos] = (byte)((value[i] >> 8) & 0xFF);
                tmp[pos + 1] = (byte)(value[i] & 0xFF);
            }
            stream.Write(tmp, 0, tmp.Length);
        }

        public static IEnumerable<string> ReadLines(this Stream stream, Encoding encoding)
        {
            using (var reader = new StreamReader(stream, encoding, false, 1024, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
