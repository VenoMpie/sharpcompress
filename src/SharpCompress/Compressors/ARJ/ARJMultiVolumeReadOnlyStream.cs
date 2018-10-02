using System;
using System.Collections.Generic;
using System.IO;
using SharpCompress.Common;
using SharpCompress.IO;
using SharpCompress.Common.ARJ;
using SharpCompress.Common.ARJ.Headers;

namespace SharpCompress.Compressors.ARJ
{
    internal class ARJMultiVolumeReadOnlyStream : MultiVolumeReadOnlyStream<ARJFilePart>
    {
        internal ARJMultiVolumeReadOnlyStream(IEnumerable<ARJFilePart> parts, IExtractionListener streamListener) : base(parts, streamListener) { }

        protected override long GetCompressedSize(ARJFilePart filePart)
        {
            return filePart.FileHeader.CompressedSize;
        }

        protected override uint GetFileCRC(ARJFilePart filePart)
        {
            return filePart.FileHeader.OriginalFileCRC32;
        }

        protected override string GetFilename(ARJFilePart filePart)
        {
            return filePart.FileHeader.FileName;
        }

        protected override long GetUncompressedSize(ARJFilePart filePart)
        {
            return filePart.FileHeader.OriginalSize;
        }

        protected override bool IsFilePartSplit(ARJFilePart filePart)
        {
            return filePart.FileHeader.Flags.HasFlag(FileHeaderFlags.ExtendedFile_ARJProtected);
        }

        protected override bool IsSalted(ARJFilePart filePart)
        {
            return filePart.FileHeader.Flags.HasFlag(FileHeaderFlags.Garbled);
        }
    }
}