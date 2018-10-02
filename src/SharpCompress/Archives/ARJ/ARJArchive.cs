using SharpCompress.Common.ARJ;
using System;
using System.Collections.Generic;
using SharpCompress.Readers;
using System.IO;
using SharpCompress.Common;
using SharpCompress.IO;
using System.Linq;
using SharpCompress.Common.ARJ.Headers;
using SharpCompress.Readers.ARJ;

namespace SharpCompress.Archives.ARJ
{
    public class ARJArchive : AbstractArchive<ARJArchiveEntry, ARJVolume>
    {
        #region Implementation

#if !NO_FILE
        internal ARJArchive(FileInfo fileInfo, ReaderOptions readerOptions) : base(ArchiveType.ARJ, fileInfo, readerOptions)
        {
        }

        protected override IEnumerable<ARJVolume> LoadVolumes(FileInfo file)
        {
            return ARJVolumeFactory.GetVolumes(StreamingMode.Seekable, file, ReaderOptions);
        }
#endif


        internal ARJArchive(IEnumerable<Stream> streams, ReaderOptions readerOptions)
            : base(ArchiveType.ARJ, streams, readerOptions)
        {
        }

        protected override IEnumerable<ARJArchiveEntry> LoadEntries(IEnumerable<ARJVolume> volumes)
        {
            return ARJEntryFactory.GetEntries(this, volumes);
        }

        protected override IEnumerable<ARJVolume> LoadVolumes(IEnumerable<Stream> streams)
        {
            return ARJVolumeFactory.GetVolumes(StreamingMode.Streaming, streams, ReaderOptions);
        }

        protected override IReader CreateReaderForSolidExtraction()
        {
            var stream = Volumes.First().Stream;
            stream.Position = 0;
            return ARJReader.Open(stream, ReaderOptions);
        }
        #endregion

        #region Identification
        public static bool IsARJFile(Stream stream, ReaderOptions options = null)
        {
            try
            {
                var headerFactory = new ARJHeaderFactory(StreamingMode.Streaming, options ?? new ReaderOptions());
                var mainHeader = headerFactory.ReadHeaders(stream).FirstOrDefault() as MainHeader;
                return mainHeader != null && mainHeader.ValidateHeaderCrc();
            }
            catch
            {
                return false;
            }
        }

#if !NO_FILE
        public static bool IsARJFile(string filePath)
        {
            return IsARJFile(new FileInfo(filePath));
        }

        public static bool IsARJFile(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                return false;
            }
            using (Stream stream = fileInfo.OpenRead())
            {
                return IsARJFile(stream);
            }
        }
#endif
        #endregion

        #region Creation

#if !NO_FILE

        /// <summary>
        /// Constructor with a FileInfo object to an existing file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="options"></param>
        public static ARJArchive Open(string filePath, ReaderOptions options = null)
        {
            filePath.CheckNotNullOrEmpty("filePath");
            return new ARJArchive(new FileInfo(filePath), options ?? new ReaderOptions());
        }

        /// <summary>
        /// Constructor with a FileInfo object to an existing file.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="options"></param>
        public static ARJArchive Open(FileInfo fileInfo, ReaderOptions options = null)
        {
            fileInfo.CheckNotNull("fileInfo");
            return new ARJArchive(fileInfo, options ?? new ReaderOptions());
        }
#endif

        /// <summary>
        /// Takes a seekable Stream as a source
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        public static ARJArchive Open(Stream stream, ReaderOptions options = null)
        {
            stream.CheckNotNull("stream");
            return Open(stream.AsEnumerable(), options ?? new ReaderOptions());
        }

        /// <summary>
        /// Takes multiple seekable Streams for a multi-part archive
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="options"></param>
        public static ARJArchive Open(IEnumerable<Stream> streams, ReaderOptions options = null)
        {
            streams.CheckNotNull("streams");
            return new ARJArchive(streams, options ?? new ReaderOptions());
        }

        #endregion
    }
}
