using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Util
{
    public class SequenceStream : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => Streams.Select(s => s.Length).Sum();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        Queue<Stream> Streams { get; }

        bool LeaveOpen { get; }

        public SequenceStream(IEnumerable<Stream> streams) : this(streams, false) { }

        public SequenceStream(IEnumerable<Stream> streams, bool leaveOpen)
        {
            Streams = new Queue<Stream>(streams);
            LeaveOpen = leaveOpen;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Streams.Count < 1)
            {
                return 0;
            }

            var read = Streams.Peek().Read(buffer, offset, count);
            if (read == 0)
            {
                var eosStream = Streams.Dequeue();
                if (!LeaveOpen)
                {
                    eosStream.Dispose();
                }

                read += Read(buffer, offset, count);
            }

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
