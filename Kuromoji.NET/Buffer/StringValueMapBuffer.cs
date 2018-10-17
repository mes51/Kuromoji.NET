using Kuromoji.NET.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Extentions;
using Kuromoji.NET.Util;

namespace Kuromoji.NET.Buffer
{
    public class StringValueMapBuffer
    {
        const int IntBytes = sizeof(int);

        const int ShortBytes = sizeof(short);

        const short KatakanaFlag = unchecked((short)0x8000);

        const short KatakanaLengthMask = 0x7fff;

        const char KatakanaBase = '\u3000';

        byte[] Buffer { get; set; }

        int Size { get; set; }

        public string this[int key]
        {
            get
            {
                var keyIndex = (key + 1) * IntBytes;
                var valueIndex = Buffer.ReadInt32(keyIndex);
                var length = Buffer.ReadInt16(valueIndex);

                if ((length & KatakanaFlag) != 0)
                {
                    length &= KatakanaLengthMask;
                    return GetKatakanaString(valueIndex + ShortBytes, length);
                }
                else
                {
                    return GetString(valueIndex + ShortBytes, length);
                }
            }
        }

        public StringValueMapBuffer(SortedDictionary<int, string> features)
        {
            Put(features);
        }

        public StringValueMapBuffer(Stream input)
        {
            Buffer = ByteBufferIO.Read(input);
            Size = Buffer.ReadInt32(0);
        }

        public void Write(Stream output)
        {
            ByteBufferIO.Write(output, Buffer);
        }

        string GetKatakanaString(int valueIndex, int length)
        {
            var chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = (char)(KatakanaBase + (Buffer[valueIndex + i] & 0xff));
            }

            return new string(chars);
        }

        unsafe string GetString(int valueIndex, int length)
        {
            fixed(byte* ptr = &Buffer[0])
            {
                var sbytePtr = (sbyte*)ptr;
                return new string(sbytePtr, valueIndex, length, Encoding.Unicode);
            }
        }

        void Put(SortedDictionary<int, string> strings)
        {
            var bufferSize = CalculateSize(strings);
            Size = strings.Count;

            Buffer = new byte[bufferSize];
            Buffer.WriteInt32(0, Size); // Set entries

            var keyIndex = IntBytes; // First key index is past size
            var entryIndex = keyIndex + Size * IntBytes;

            foreach (var str in strings.Values)
            {
                Buffer.WriteInt32(keyIndex, entryIndex);
                entryIndex = Put(entryIndex, str);
                keyIndex += IntBytes;
            }
        }

        int Put(int index, string value)
        {
            bool katakana = ScriptUtils.IsKatakana(value);
            byte[] bytes;
            short length;

            if (katakana)
            {
                bytes = GetKatakanaBytes(value);
                length = (short)(bytes.Length | KatakanaFlag & 0xffff);
            }
            else
            {
                bytes = GetBytes(value);
                length = (short)bytes.Length;
            }
            
            Buffer.WriteInt16(index, length);
            System.Buffer.BlockCopy(bytes, 0, Buffer, index + ShortBytes, bytes.Length);

            return index + ShortBytes + bytes.Length;
        }

        static int CalculateSize(SortedDictionary<int, string> values)
        {
            int size = IntBytes + values.Count * IntBytes;

            foreach (var value in values.Values)
            {
                size += ShortBytes + GetByteSize(value);
            }
            return size;
        }

        static int GetByteSize(string str)
        {
            if (ScriptUtils.IsKatakana(str))
            {
                return str.Length;
            }

            return GetByteCount(str);
        }

        static byte[] GetKatakanaBytes(string str)
        {
            var bytes = new byte[str.Length];

            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];

                bytes[i] = (byte)(c - KatakanaBase);
            }

            return bytes;
        }

        static byte[] GetBytes(string str)
        {
            return Encoding.Unicode.GetBytes(str);
        }

        static int GetByteCount(string str)
        {
            return Encoding.Unicode.GetByteCount(str);
        }
    }
}
