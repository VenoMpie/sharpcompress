using SharpCompress.Common.ARJ;
using System;
using System.IO;
using System.Linq;
using SharpCompress.Common;
using System.Collections.Generic;
using SharpCompress.Common.ARJ.Headers;
using SharpCompress.IO;
using SharpCompress.Compressors.ARJ;

namespace SharpCompress.Archives.ARJ
{
    public class ARJArchiveEntry : ARJEntry, IArchiveEntry
    {
        private readonly ICollection<ARJFilePart> parts;
        private readonly ARJArchive archive;

        internal ARJArchiveEntry(ARJArchive archive, IEnumerable<ARJFilePart> parts) : base(parts)
        {
            this.parts = parts.ToList();
            this.archive = archive;
        }

        public bool IsComplete => !parts.All(a => FlagUtility.HasFlag(a.FileHeader.Flags, FileHeaderFlags.ExtendedFile_ARJProtected));

        public override long CompressedSize
        {
            get
            {
                CheckIncomplete();
                return parts.Aggregate(0L, (total, fp) => { return total + fp.FileHeader.CompressedSize; });
            }
        }

        public IArchive Archive => archive;

        public Stream OpenEntryStream()
        {
            //TODO: Hack to only get single volume stored file for now
            //return new ReadOnlySubStream(parts.First().GetRawStream(), parts.First().FileHeader.CompressedSize);
            return new ARJMultiVolumeReadOnlyStream(Parts.Cast<ARJFilePart>(), archive);
        }

        private void CheckIncomplete()
        {
            if (!IsComplete)
            {
                throw new IncompleteArchiveException("ArchiveEntry is incomplete and cannot perform this operation.");
            }
        }
    }
}
