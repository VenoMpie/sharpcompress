using SharpCompress.Common.ARJ.Headers;
using SharpCompress.IO;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpCompress.Common.ARJ
{
    public class ARJVolume : Volume
    {
        internal MainHeader MainHeader { get; private set; }

        private readonly ARJHeaderFactory headerFactory;

        internal ARJVolume(StreamingMode mode, Stream stream, ReaderOptions readerOptions) : base(stream, readerOptions)
        {
            headerFactory = new ARJHeaderFactory(mode, readerOptions);
            GetMainHeader();
        }

        public override bool IsFirstVolume
        {
            get
            {
                //What a hacky way to do this, it's going to fail with Numbered Multi Volumes ... but it is what it is
                if (FlagUtility.HasFlag(MainHeader.MainFlags, MainHeaderExtendedFlags.AltVolumeName))
                {
                    string FileNameWithoutExtension = Path.GetFileNameWithoutExtension(MainHeader.FileName);
                    int startIndex = FileNameWithoutExtension.Length > 3 ? FileNameWithoutExtension.Length - 3 : FileNameWithoutExtension.Length;
                    return !int.TryParse(FileNameWithoutExtension.Substring(startIndex, 3), out int res);
                }
                else
                    return string.Compare(Path.GetExtension(MainHeader.FileName), ".ARJ", StringComparison.OrdinalIgnoreCase) == 0;
            }
        }
        public override bool IsMultiVolume => FlagUtility.HasFlag(MainHeader.Flags, FileHeaderFlags.Volume) || !IsFirstVolume;

        public string Comment => MainHeader.Comment;

        private void GetMainHeader()
        {
            Stream.Position = 0;
            ARJHeader header = headerFactory.ReadHeaders(Stream).FirstOrDefault();
            if (header != null && header is MainHeader)
            {
                MainHeader = header as MainHeader;
            }
            else
                throw new BadHeaderException();

            Stream.Position = 0;
        }
        internal IEnumerable<ARJFilePart> GetVolumeFileParts()
        {
            foreach (FileHeader header in headerFactory.ReadHeaders(Stream).OfType<FileHeader>())
            {
                yield return new ARJFilePart(header, Stream, ReaderOptions.Password);
            }
        }
    }
}
