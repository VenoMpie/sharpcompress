using System;

namespace SharpCompress.Common.ARJ.Headers
{
    public class MainHeader : ARJHeader
    {
        internal DateTime CreatedDate { get; set; }
        internal DateTime ModifiedDate { get; set; }
        internal uint ArchiveSize { get; set; }
        internal uint SecurityEnvelopePosition { get; set; }
        internal ushort FileEntryPosition { get; set; }
        internal ushort SecurityEnvelopeLength { get; set; }
        internal EncryptionVersion EncryptionVersion { get; set; }
        internal byte LastChapter { get; set; }

        //Extra Data
        internal byte ProtectionFactor { get; set; }
        internal MainHeaderExtendedFlags MainFlags { get; set; }
        internal ushort SpareBytes { get; set; }

        protected override void ReadDerived()
        {
            //There's a reserved byte in the file which we ignore (byte 8 so the timestamp starts from byte 9)
            CreatedDate = GetDateTime(BitConverter.ToInt32(HeaderRecord, 8));
            try
            {
                //Not all archives have the dates properly
                ModifiedDate = GetDateTime(BitConverter.ToInt32(HeaderRecord, 12));
            }
            catch { }

            ArchiveSize = BitConverter.ToUInt32(HeaderRecord, 16);
            SecurityEnvelopePosition = BitConverter.ToUInt32(HeaderRecord, 20);
            FileEntryPosition = BitConverter.ToUInt16(HeaderRecord, 24);
            SecurityEnvelopeLength = BitConverter.ToUInt16(HeaderRecord, 26);
            EncryptionVersion = (EncryptionVersion)HeaderRecord[28];
            LastChapter = HeaderRecord[29];

            //Version is probably > 2.62 as bytes 32-33 is a reserved word
            if (FirstHeaderSize > 30 && HeaderRecord[32] == 0 && HeaderRecord[33] == 0)
            {
                ProtectionFactor = HeaderRecord[30];
                MainFlags = (MainHeaderExtendedFlags)HeaderRecord[31];
            }
        }
    }
}
