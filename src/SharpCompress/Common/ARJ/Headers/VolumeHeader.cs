using System;

namespace SharpCompress.Common.ARJ.Headers
{
    public class VolumeHeader : ARJHeader
    {
        internal DateTime CreatedDate { get; set; }
        internal uint OriginalFileCRC32 { get; set; }
        internal ushort FileEntryPosition { get; set; }
        internal ushort FileAccessMode { get; set; }
        internal byte ChapterRangeStart { get; set; }
        internal byte ChapterRangeEnd { get; set; }

        //Extra Data
        internal uint ExtendedFilePosition { get; set; }

        protected override void ReadDerived()
        {
            //There's a reserved byte in the file which we ignore (byte 8 so the timestamp starts from byte 9)
            CreatedDate = GetDateTime(BitConverter.ToInt32(HeaderRecord, 8));
            OriginalFileCRC32 = BitConverter.ToUInt32(HeaderRecord, 20);
            FileEntryPosition = BitConverter.ToUInt16(HeaderRecord, 24);
            FileAccessMode = BitConverter.ToUInt16(HeaderRecord, 26);
            ChapterRangeStart = HeaderRecord[28];
            ChapterRangeEnd = HeaderRecord[29];
        }
    }
}
