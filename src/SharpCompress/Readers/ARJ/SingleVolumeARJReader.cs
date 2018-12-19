using System.IO;
using SharpCompress.Common;
using SharpCompress.Common.ARJ;

namespace SharpCompress.Readers.ARJ
{
    internal class SingleVolumeARJReader : ARJReader
    {
        private readonly Stream stream;

        internal SingleVolumeARJReader(Stream stream, ReaderOptions options)
            : base(options)
        {
            this.stream = stream;
        }

        internal override void ValidateArchive(ARJVolume archive)
        {
            if (archive.IsMultiVolume)
            {
                throw new MultiVolumeExtractionException(
                                                         "Streamed archive is a Multi-volume archive.  Use different ARJReader method to extract.");
            }
        }

        protected override Stream RequestInitialStream()
        {
            return stream;
        }
    }
}