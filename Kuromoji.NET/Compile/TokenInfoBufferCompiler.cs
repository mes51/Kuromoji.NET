using Kuromoji.NET.Buffer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Extentions;
using Kuromoji.NET.IO;

namespace Kuromoji.NET.Compile
{
    public class TokenInfoBufferCompiler : ICompiler, IDisposable
    {
        const int IntBytes = sizeof(int);

        const int ShortBytes = sizeof(short);

        byte[] Buffer { get; set; }

        Stream Output { get; }

        bool LeaveOpen { get; }

        bool Disposed { get; set; }

        public TokenInfoBufferCompiler(Stream output, List<BufferEntry> entries) : this(output, entries, false) { }

        public TokenInfoBufferCompiler(Stream output, List<BufferEntry> entries, bool leaveOpen)
        {
            Output = output;
            LeaveOpen = leaveOpen;
            PutEntries(entries);
        }

        public void PutEntries(List<BufferEntry> entries)
        {
            var size = CalculateEntriesSize(entries) * 2;

            using (var ms = new MemoryStream())
            {
                ms.Write(size);
                ms.Write(entries.Count);

                var firstEntry = entries[0];
                ms.Write(firstEntry.TokenInfo.Count);
                ms.Write(firstEntry.PosInfo.Count);
                ms.Write(firstEntry.Features.Count);

                foreach (var entry in entries)
                {
                    foreach (var s in entry.TokenInfo)
                    {
                        ms.Write(s);
                    }

                    foreach (var b in entry.PosInfo)
                    {
                        ms.WriteByte(b);
                    }

                    foreach (var feature in entry.Features)
                    {
                        ms.Write(feature);
                    }
                }

                var bufferSize = size + IntBytes * 4;
                Buffer = ms.ToArray().Concat(Enumerable.Repeat((byte)0, bufferSize)).Take(bufferSize).ToArray();
            }
        }

        int CalculateEntriesSize(List<BufferEntry> entries)
        {
            if (entries.Count == 0)
            {
                return 0;
            }
            else
            {
                var size = 0;
                var entry = entries[0];
                size += entry.TokenInfo.Count * ShortBytes + ShortBytes;
                size += entry.PosInfo.Count;
                size += entry.Features.Count * IntBytes;
                size *= entries.Count;
                return size;
            }
        }

        public void Compile()
        {
            ByteBufferIO.Write(Output, Buffer);
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                if (!LeaveOpen)
                {
                    Output.Dispose();
                }
                Disposed = true;
            }
        }
    }
}
