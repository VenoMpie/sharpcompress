using SharpCompress.Common.ARJ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SharpCompress.Common;

namespace SharpCompress.Readers.ARJ
{
    public abstract class ARJReader : AbstractReader<ARJEntry, ARJVolume>
    {
        internal ARJReader(ReaderOptions options) : base(options, ArchiveType.ARJ) { }

        #region Implementation
        private ARJVolume volume;
        public override ARJVolume Volume => volume;

        protected override IEnumerable<ARJEntry> GetEntries(Stream stream)
        {
            volume = new ARJVolume(IO.StreamingMode.Streaming, stream, Options);
            ValidateArchive(volume);
            yield return new ARJEntry(volume.GetVolumeFileParts());
        }
        #endregion

        internal abstract void ValidateArchive(ARJVolume archive);

        #region Open

        /// <summary>
        /// Opens a RarReader for Non-seeking usage with a single volume
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ARJReader Open(Stream stream, ReaderOptions options = null)
        {
            stream.CheckNotNull("stream");
            return new SingleVolumeARJReader(stream, options ?? new ReaderOptions());
        }

        /// <summary>
        /// Opens a RarReader for Non-seeking usage with multiple volumes
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ARJReader Open(IEnumerable<Stream> streams, ReaderOptions options = null)
        {
            streams.CheckNotNull("streams");
            return new MultiVolumeARJReader(streams, options ?? new ReaderOptions());
        }

        #endregion

        protected virtual IEnumerable<FilePart> CreateFilePartEnumerableForCurrentEntry()
        {
            return Entry.Parts;
        }

        protected override EntryStream GetEntryStream()
        {
            throw new Exception();
            //return CreateEntryStream(new RarCrcStream(pack, Entry.FileHeader,
            //                                       new MultiVolumeReadOnlyStream(
            //                                                                     CreateFilePartEnumerableForCurrentEntry().Cast<RarFilePart>(), this)));
        }
    }
}
