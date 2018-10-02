using SharpCompress.Common.ARJ.Headers;
using System;
using System.IO;

namespace SharpCompress.Common.ARJ
{
    internal class ARJFilePart : FilePart
    {
        internal FileHeader FileHeader { get; }

        private readonly Stream stream;
        private readonly string password;

        internal protected ARJFilePart(FileHeader fh, Stream stream, string password) : base(new ArchiveEncoding() { Default = System.Text.Encoding.UTF8 })
        {
            FileHeader = fh;
            this.stream = stream;
            this.password = password;
        }

        internal override string FilePartName => FileHeader.FileName;

        internal override Stream GetCompressedStream()
        {
            //if (FileHeader.Method_SecurityVersion != 0)
            //{
                stream.Position = FileHeader.FileStartPosition;
                return stream;
            //}
            //return null;
        }

        internal override Stream GetRawStream()
        {
            //if (FileHeader.Method_SecurityVersion == 0)
            //{
                stream.Position = FileHeader.FileStartPosition;
                return stream;
            //}
            //return null;
        }
    }
}
