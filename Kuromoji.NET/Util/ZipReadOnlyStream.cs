using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Util
{
    public class ZipReadOnlyStream : Stream
    {
        ZipArchive Archive { get; }

        ZipArchiveEntry Entry { get; }

        Stream EntryStream { get; }

        bool Disposed { get; set; }

        public ZipReadOnlyStream(string zipFilePath, string entryPath)
        {
            Archive = ZipFile.OpenRead(zipFilePath);
            Entry = Archive.GetEntry(entryPath);
            EntryStream = Entry.Open();
        }

        public override bool CanRead => EntryStream.CanRead;

        public override bool CanSeek => EntryStream.CanSeek;

        public override bool CanWrite => false;

        public override long Length => EntryStream.Length;

        public override long Position
        {
            get => EntryStream.Position;
            set => EntryStream.Position = value;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return EntryStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return EntryStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!Disposed)
            {
                EntryStream.Dispose();
                Archive.Dispose();
                Disposed = true;
            }
        }
    }
}
