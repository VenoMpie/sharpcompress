using System;

namespace SharpCompress.Common.ARJ.Headers
{
    internal enum HeaderType
    {
        ArchiveMainHeader = 0,
        LocalFileHeader = 1,
        ChapterHeader = 2
    }

    internal enum FileTypeEnum
    {
        Binary = 0,
        SevenBitText = 1,
        MainHeader = 2,
        Directory = 3,
        VolumeLabel = 4,
        Chapter = 5
    }

    [Flags]
    internal enum FileAttributeFlags : ushort
    {
        ReadOnly = 0x01,
        Hidden = 0x02,
        System = 0x04,
        VolumeLabel = 0x08,
        Directory = 0x10,
        Archive = 0x20
    }

    internal enum HostOSEnum : byte
    {
        MSDOS = 0,
        Primos = 1,
        Unix = 2,
        Amiga = 3,
        MacOS = 4,
        OS2 = 5,
        AppleGS = 6,
        AtariST = 7,
        Next = 8,
        VAXVMS = 9
    }

    internal enum EncryptionVersion : byte
    {
        Old = 0,
        Old1 = 1,
        New = 2,
        Reserved = 3,
        GOST40BitKey = 4,
    }

    [Flags]
    internal enum FileHeaderFlags : byte
    {
        Garbled = 0x01,
        Old_Secured_ANSIPage_Flag = 0x02,
        Volume = 0x04,
        ExtendedFile_ARJProtected = 0x08,
        PathSym = 0x10,
        Backup = 0x20,
        Altname_DualName_Secured = 0x40,
    }

    [Flags]
    internal enum MainHeaderExtendedFlags : byte
    {
        AltVolumeName = 0x01,
        Reserved = 0x02,
    }

    public class Constants
    {
        internal const uint CRC_MASK = 0xFFFFFFFF;
        internal const short HEADER_ID_HI = 0xEA;
        internal const short HEADER_ID_LO = 0x60;
        internal const short FIRST_HDR_SIZE = 30;
        internal const short FNAME_MAX = 512;
        internal const short COMMENT_MAX = 2048;
        internal const long MAXSFX = 25000L;
        //Maximum Header Size is 2600
        internal const short HEADERSIZE_MAX = (FIRST_HDR_SIZE + 10 + FNAME_MAX + COMMENT_MAX);
    }
}