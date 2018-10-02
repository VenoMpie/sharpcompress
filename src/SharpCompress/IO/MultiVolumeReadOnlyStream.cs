using System;
using System.Collections.Generic;
using System.IO;
using SharpCompress.Common;
using SharpCompress.Common.Rar;
using SharpCompress.Common.Rar.Headers;

namespace SharpCompress.IO
{
    internal abstract class MultiVolumeReadOnlyStream<T> : Stream where T : FilePart
    {
        private long currentPosition;
        private long maxPosition;

        private IEnumerator<T> filePartEnumerator;
        private Stream currentStream;

        private readonly IExtractionListener streamListener;

        private long currentPartTotalReadBytes;
        private long currentEntryTotalReadBytes;

        internal MultiVolumeReadOnlyStream(IEnumerable<T> parts, IExtractionListener streamListener)
        {
            this.streamListener = streamListener;

            filePartEnumerator = parts.GetEnumerator();
            filePartEnumerator.MoveNext();
            InitializeNextFilePart();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (filePartEnumerator != null)
                {
                    filePartEnumerator.Dispose();
                    filePartEnumerator = null;
                }
                if (currentStream != null)
                {
                    currentStream.Dispose();
                    currentStream = null;
                }
            }
        }

        protected abstract long GetCompressedSize(T filePart);
        protected abstract long GetUncompressedSize(T filePart);
        protected abstract uint GetFileCRC(T filePart);
        protected abstract string GetFilename(T filePart);
        protected abstract bool IsFilePartSplit(T filePart);
        protected abstract bool IsSalted(T filePart);

        private void InitializeNextFilePart()
        {
            long compressedSize = GetCompressedSize(filePartEnumerator.Current);
            long uncompressedSize = GetUncompressedSize(filePartEnumerator.Current);
            uint crc = GetFileCRC(filePartEnumerator.Current);

            maxPosition = compressedSize;
            currentPosition = 0;
            if (currentStream != null)
            {
                currentStream.Dispose();
            }
            currentStream = filePartEnumerator.Current.GetCompressedStream();

            currentPartTotalReadBytes = 0;

            CurrentCrc = crc;

            streamListener.FireFilePartExtractionBegin(filePartEnumerator.Current.FilePartName,
                                                       compressedSize,
                                                       uncompressedSize);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalRead = 0;
            int currentOffset = offset;
            int currentCount = count;
            while (currentCount > 0)
            {
                int readSize = currentCount;
                if (currentCount > maxPosition - currentPosition)
                {
                    readSize = (int)(maxPosition - currentPosition);
                }

                int read = currentStream.Read(buffer, currentOffset, readSize);
                if (read < 0)
                {
                    throw new EndOfStreamException();
                }

                currentPosition += read;
                currentOffset += read;
                currentCount -= read;
                totalRead += read;
                if (((maxPosition - currentPosition) == 0)
                    && IsFilePartSplit(filePartEnumerator.Current))
                {
                    if (IsSalted(filePartEnumerator.Current))
                    {
                        throw new InvalidFormatException("Sharpcompress currently does not support multi-volume decryption.");
                    }
                    string fileName = GetFilename(filePartEnumerator.Current);
                    if (!filePartEnumerator.MoveNext())
                    {
                        throw new InvalidFormatException(
                                                         "Multi-part file is incomplete.  Entry expects a new volume: " + fileName);
                    }
                    InitializeNextFilePart();
                }
                else
                {
                    break;
                }
            }
            currentPartTotalReadBytes += totalRead;
            currentEntryTotalReadBytes += totalRead;
            streamListener.FireCompressedBytesRead(currentPartTotalReadBytes, currentEntryTotalReadBytes);
            return totalRead;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public uint CurrentCrc { get; private set; }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}