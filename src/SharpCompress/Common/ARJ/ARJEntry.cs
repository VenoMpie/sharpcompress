using SharpCompress.Archives.ARJ;
using SharpCompress.Common.ARJ.Headers;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SharpCompress.Common.ARJ
{
    public class ARJEntry : Entry
    {
        internal FileHeader FileHeader { get { return parts.Last().FileHeader; } }

        private readonly ICollection<ARJFilePart> parts;

        internal ARJEntry(IEnumerable<ARJFilePart> parts)
        {
            this.parts = parts.ToList();
        }

        public override long Crc => FileHeader.OriginalFileCRC32;

        public override string Key => FileHeader.FileName;

        public override long CompressedSize => parts.Sum(a => a.FileHeader.CompressedSize);

        public override CompressionType CompressionType => CompressionType.ARJ;

        public override long Size => parts.Sum(a => a.FileHeader.OriginalSize);

        public override DateTime? LastModifiedTime => FileHeader.ModifiedDate;

        public override DateTime? CreatedTime => FileHeader.CreatedDate;

        public override DateTime? LastAccessedTime => FileHeader.AccessedDate;

        public override DateTime? ArchivedTime => FileHeader.CreatedDate;

        public override bool IsEncrypted => FileHeader.Flags.HasFlag(FileHeaderFlags.Garbled);

        public override bool IsDirectory => FileHeader.FileType.HasFlag(FileTypeEnum.Directory);

        public override bool IsSplitAfter => FileHeader.Flags.HasFlag(FileHeaderFlags.Volume);

        internal override IEnumerable<FilePart> Parts => parts.Cast<FilePart>();
    }
}
