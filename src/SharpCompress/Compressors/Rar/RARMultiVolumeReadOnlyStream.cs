using System;
using System.Collections.Generic;
using System.IO;
using SharpCompress.Common;
using SharpCompress.Common.Rar;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.IO;

namespace SharpCompress.Compressors.Rar
{
    internal class RARMultiVolumeReadOnlyStream : MultiVolumeReadOnlyStream<RarFilePart>
    {
        internal RARMultiVolumeReadOnlyStream(IEnumerable<RarFilePart> parts, IExtractionListener streamListener) : base(parts, streamListener) { }

        protected override long GetCompressedSize(RarFilePart filePart)
        {
            return filePart.FileHeader.CompressedSize;
        }

        protected override uint GetFileCRC(RarFilePart filePart)
        {
            return filePart.FileHeader.FileCrc;
        }

        protected override string GetFilename(RarFilePart filePart)
        {
            return filePart.FileHeader.FileName;
        }

        protected override long GetUncompressedSize(RarFilePart filePart)
        {
            return filePart.FileHeader.UncompressedSize;
        }

        protected override bool IsFilePartSplit(RarFilePart filePart)
        {
            return filePart.FileHeader.HasFlag(FileFlagsV4.SPLIT_AFTER);
        }

        protected override bool IsSalted(RarFilePart filePart)
        {
            return filePart.FileHeader.R4Salt != null;
        }
    }
}