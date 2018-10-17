using Kuromoji.NET.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Extentions;

namespace Kuromoji.NET.Buffer
{
    public class TokenInfoBuffer
    {
        const int IntBytes = sizeof(int);
        const int ShortBytes = sizeof(short);

        int TokenInfoCount { get; }

        int PosInfoCount { get; }

        int FeatureCount { get; }

        int EntrySize { get; }

        byte[] Buffer { get; }

        public TokenInfoBuffer(Stream input)
        {
            Buffer = ByteBufferIO.Read(input);
            TokenInfoCount = Buffer.ReadInt32(IntBytes * 2);
            PosInfoCount = Buffer.ReadInt32(IntBytes * 3);
            FeatureCount = Buffer.ReadInt32(IntBytes * 4);
            EntrySize = TokenInfoCount * ShortBytes + PosInfoCount + FeatureCount * IntBytes;
        }

        public BufferEntry LookupEntry(int offset)
        {
            var entry = new BufferEntry
            {
                TokenInfos = new short[TokenInfoCount],
                PosInfos = new byte[PosInfoCount],
                FeatureInfos = new int[FeatureCount]
            };

            var position = GetPosition(offset, EntrySize);

            // Get left id, right id and word cost
            for (var i = 0; i < TokenInfoCount; i++)
            {
                entry.TokenInfos[i] = Buffer.ReadInt16(position + i * ShortBytes);
            }

            // Get part of speech tags values (not strings yet)
            for (int i = 0; i < PosInfoCount; i++)
            {
                entry.PosInfos[i] = Buffer[position + TokenInfoCount * ShortBytes + i];
            }

            // Get field value references (string references)
            for (int i = 0; i < FeatureCount; i++)
            {
                entry.FeatureInfos[i] = Buffer.ReadInt32(position + TokenInfoCount * ShortBytes + PosInfoCount + i * IntBytes);
            }

            return entry;
        }

        public int LookupTokenInfo(int offset, int i)
        {
            int position = GetPosition(offset, EntrySize);
            return Buffer.ReadInt16(position + i * ShortBytes);
        }

        public int LookupPartOfSpeechFeature(int offset, int i)
        {
            int position = GetPosition(offset, EntrySize);
            return 0xff & Buffer[position + TokenInfoCount * ShortBytes + i];
        }

        public int LookupFeature(int offset, int i)
        {
            int position = GetPosition(offset, EntrySize);
            return Buffer.ReadInt32(position + TokenInfoCount * ShortBytes + PosInfoCount + (i - PosInfoCount) * IntBytes);
        }

        public bool isPartOfSpeechFeature(int i)
        {
            return i < PosInfoCount;
        }

        int GetPosition(int offset, int entrySize)
        {
            return offset * entrySize + IntBytes * 5;
        }
    }
}
