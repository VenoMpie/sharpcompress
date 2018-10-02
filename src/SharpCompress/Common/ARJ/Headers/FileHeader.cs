using System;
using System.IO;
using System.Linq;

namespace SharpCompress.Common.ARJ.Headers
{
    public class FileHeader : ARJHeader
    {
        internal DateTime ModifiedDate { get; set; }
        internal uint CompressedSize { get; set; }
        internal uint OriginalSize { get; set; }
        internal uint OriginalFileCRC32 { get; set; }
        internal ushort FileEntryPosition { get; set; }
        internal ushort FileAccessMode { get; set; }
        internal byte FirstChapter { get; set; }
        internal byte LastChapter { get; set; }

        //Extra Data
        internal uint ExtendedFilePosition { get; set; }
        internal DateTime AccessedDate { get; set; }
        internal DateTime CreatedDate { get; set; }
        internal uint OriginalSizeIncludingVolumes { get; set; }

        //Unpacking
        internal Stream PackedStream { get; set; }
        internal long FileStartPosition { get; set; }

        protected override void ReadDerived()
        {
            //There's a reserved byte in the file which we ignore (byte 8 so the timestamp starts from byte 9)

            //Modified date is only from v6 onwards.
            if (ArchiverVersion >= 6)
                ModifiedDate = GetDateTime(BitConverter.ToInt32(HeaderRecord, 8));

            CompressedSize = BitConverter.ToUInt32(HeaderRecord, 12);
            OriginalSize = BitConverter.ToUInt32(HeaderRecord, 16);
            OriginalFileCRC32 = BitConverter.ToUInt32(HeaderRecord, 20);
            FileEntryPosition = BitConverter.ToUInt16(HeaderRecord, 24);
            FileAccessMode = BitConverter.ToUInt16(HeaderRecord, 26);
            FirstChapter = HeaderRecord[28];
            LastChapter = HeaderRecord[29];

            if (OriginalSize < 0 || CompressedSize < 0) throw new HeaderCRCError();

            //Check if it's an extended file
            if (FlagUtility.HasFlag((long)Flags, (long)(FileHeaderFlags.ExtendedFile_ARJProtected)))
            {
                ExtendedFilePosition = BitConverter.ToUInt32(HeaderRecord, 30);
                if (FileEntryPosition == 0) FileEntryPosition = (ushort)ExtendedFilePosition;
            }

            //12 bytes may be present which might either be 0 or valid dates
            //Check if it's a new file (Archive Version >= 9, the bytes won't be present before)
            //If FirstHeaderSize is Greater than 34 (we have to add the extended header) then the 12 bytes are definately present
            if (ArchiverVersion >= 9 && FirstHeaderSize > 34)
            {
                byte[] checkArray = new byte[12];
                Array.Copy(HeaderRecord, 34, checkArray, 0, 12);
                OriginalSizeIncludingVolumes = BitConverter.ToUInt32(checkArray, 8);
                try
                {
                    if (checkArray.Where((a, index) => index < 8).Any(a => a != 0))
                    {
                        //See if we can parse the Accessed Date and Created Date
                        AccessedDate = GetDateTime(BitConverter.ToInt32(checkArray, 0));
                        ModifiedDate = GetDateTime(BitConverter.ToInt32(checkArray, 4));
                    }
                }
                catch { }
            }
        }
    }
}
