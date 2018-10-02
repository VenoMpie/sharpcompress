using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpCompress.IO
{
    public class MultiVolumeStreamEnumerator<E, V, R> : IEnumerable<FilePart>, IEnumerator<FilePart> where E : Entry where V : Volume where R : AbstractReader<E, V>
    {
        private readonly R reader;
        private readonly IEnumerator<Stream> nextReadableStreams;
        private Stream tempStream;
        private bool isFirst = true;

        internal MultiVolumeStreamEnumerator(R r, IEnumerator<Stream> nextReadableStreams,
                                             Stream tempStream)
        {
            reader = r;
            this.nextReadableStreams = nextReadableStreams;
            this.tempStream = tempStream;
        }

        public IEnumerator<FilePart> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public FilePart Current { get; private set; }

        public void Dispose()
        {
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (isFirst)
            {
                Current = reader.Entry.Parts.First();
                isFirst = false; //first stream already to go
                return true;
            }

            if (!reader.Entry.IsSplit)
            {
                return false;
            }
            if (tempStream != null)
            {
                reader.LoadStreamForReading(tempStream);
                tempStream = null;
            }
            else if (!nextReadableStreams.MoveNext())
            {
                throw new MultiVolumeExtractionException("No stream provided when requested by MultiVolumeReader");
            }
            else
            {
                reader.LoadStreamForReading(nextReadableStreams.Current);
            }

            Current = reader.Entry.Parts.First();
            return true;
        }

        public void Reset()
        {
        }
    }
}
