using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Common;
using SharpCompress.Common.ARJ;
using SharpCompress.IO;

namespace SharpCompress.Readers.ARJ
{
    internal class MultiVolumeARJReader : ARJReader
    {
        private readonly IEnumerator<Stream> streams;
        private Stream tempStream;

        internal MultiVolumeARJReader(IEnumerable<Stream> streams, ReaderOptions options)
            : base(options)
        {
            this.streams = streams.GetEnumerator();
        }

        internal override void ValidateArchive(ARJVolume archive)
        {
        }

        protected override Stream RequestInitialStream()
        {
            if (streams.MoveNext())
            {
                return streams.Current;
            }
            throw new MultiVolumeExtractionException("No stream provided when requested by MultiVolumeARJReader");
        }

        internal override bool NextEntryForCurrentStream()
        {
            if (!base.NextEntryForCurrentStream())
            {
                //if we're got another stream to try to process then do so
                if (streams.MoveNext() && LoadStreamForReading(streams.Current))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        protected override IEnumerable<FilePart> CreateFilePartEnumerableForCurrentEntry()
        {
            var enumerator = new MultiVolumeStreamEnumerator<ARJEntry, ARJVolume, MultiVolumeARJReader>(this, streams, tempStream);
            tempStream = null;
            return enumerator;
        }
    }
}