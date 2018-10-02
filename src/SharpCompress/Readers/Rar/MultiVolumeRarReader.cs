﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Common;
using SharpCompress.Common.Rar;
using SharpCompress.IO;

namespace SharpCompress.Readers.Rar
{
    internal class MultiVolumeRarReader : RarReader
    {
        private readonly IEnumerator<Stream> streams;
        private Stream tempStream;

        internal MultiVolumeRarReader(IEnumerable<Stream> streams, ReaderOptions options)
            : base(options)
        {
            this.streams = streams.GetEnumerator();
        }

        internal override void ValidateArchive(RarVolume archive)
        {
        }

        internal override Stream RequestInitialStream()
        {
            if (streams.MoveNext())
            {
                return streams.Current;
            }
            throw new MultiVolumeExtractionException("No stream provided when requested by MultiVolumeRarReader");
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
            var enumerator = new MultiVolumeStreamEnumerator<RarReaderEntry, RarVolume, MultiVolumeRarReader>(this, streams, tempStream);
            tempStream = null;
            return enumerator;
        }
    }
}