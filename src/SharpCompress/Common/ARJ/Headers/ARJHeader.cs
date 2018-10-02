using SharpCompress.Common.ARJ;
using SharpCompress.Compressors.Rar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpCompress.Common.ARJ.Headers
{
    public class ARJHeader
    {
        internal ArchiveEncoding ArchiveEncoding { get; private set; }

        internal ushort HeaderSize { get; set; }
        internal byte FirstHeaderSize { get; set; }
        internal byte ArchiverVersion { get; set; }
        internal byte MinimumArchiverVersion { get; set; }
        internal HostOSEnum HostOS { get; set; }
        internal FileHeaderFlags Flags { get; set; }
        internal byte Method_SecurityVersion { get; set; }
        internal FileTypeEnum FileType { get; set; }

        internal byte[] HeaderRecord { get; set; }

        internal string FileName { get; set; }
        internal string Comment { get; set; }
        internal uint HeaderCRC32 { get; set; }

        //Extended Header
        internal ushort ExtendedHeaderSize { get; set; }
        internal byte[] ExtendedHeader { get; set; }
        internal uint ExtendedHeaderCRC { get; set; }

        internal ARJHeader() { }

        private void FillBase(ARJHeader baseHeader)
        {
            HeaderSize = baseHeader.HeaderSize;
            FirstHeaderSize = baseHeader.FirstHeaderSize;
            ArchiverVersion = baseHeader.ArchiverVersion;
            MinimumArchiverVersion = baseHeader.MinimumArchiverVersion;
            HostOS = baseHeader.HostOS;
            Flags = baseHeader.Flags;
            Method_SecurityVersion = baseHeader.Method_SecurityVersion;
            FileType = baseHeader.FileType;
            HeaderRecord = baseHeader.HeaderRecord;
            ExtendedHeaderSize = baseHeader.ExtendedHeaderSize;
            ArchiveEncoding = baseHeader.ArchiveEncoding;
        }

        internal static ARJHeader Create(BinaryReader reader, ArchiveEncoding archiveEncoding)
        {
            try
            {
                ARJHeader header = new ARJHeader
                {
                    ArchiveEncoding = archiveEncoding
                };

                header.FindAndReadFromReader(reader);

                return header;
            }
            catch (EndOfStreamException)
            {
                return null;
            }
        }

        private void FindAndReadFromReader(BinaryReader reader)
        {
            while (reader.BaseStream.Position <= reader.BaseStream.Length)
            {
                byte b = reader.ReadByte();
                if (b == Constants.HEADER_ID_LO)
                {
                    if ((b = reader.ReadByte()) == Constants.HEADER_ID_HI) //We need to check the second byte as well as the ID is 0x60EA
                    {
                        HeaderSize = reader.ReadUInt16();
                        if (HeaderSize > Constants.HEADERSIZE_MAX)
                            continue;
                        else if (HeaderSize == 0)
                            //End of Archive
                            break;
                        else
                        {
                            HeaderRecord = reader.ReadBytes(HeaderSize);

                            FirstHeaderSize = HeaderRecord[0];
                            ArchiverVersion = HeaderRecord[1];
                            MinimumArchiverVersion = HeaderRecord[2];
                            HostOS = (HostOSEnum)HeaderRecord[3];
                            Flags = (FileHeaderFlags)HeaderRecord[4];
                            Method_SecurityVersion = HeaderRecord[5];
                            FileType = (FileTypeEnum)HeaderRecord[6];

                            break;
                        }
                    }
                }
            }
        }
        protected virtual void ReadDerived()
        {
            throw new NotImplementedException();
        }

        private void ReadLastFromReader(BinaryReader reader)
        {
            string[] fileAndComment = ArchiveEncoding.GetEncoding().GetString(HeaderRecord, FirstHeaderSize, HeaderRecord.Length - FirstHeaderSize).Split('\0');
            FileName = fileAndComment[0];
            Comment = fileAndComment[1];
            HeaderCRC32 = reader.ReadUInt32();
            ExtendedHeaderSize = reader.ReadUInt16();
            //Skip the extended header for now
            if (ExtendedHeaderSize > 0)
                reader.BaseStream.Seek(ExtendedHeaderSize, SeekOrigin.Current);
        }

        internal T PromoteHeader<T>(BinaryReader reader)
            where T : ARJHeader, new()
        {
            T header = new T();
            header.FillBase(this);

            header.ReadDerived();
            header.ReadLastFromReader(reader);

            if (!header.ValidateHeaderCrc())
                return null;

            return header;
        }

        public bool ValidateHeaderCrc()
        {
            uint crc = ~RarCRC.CheckCrc(Constants.CRC_MASK, HeaderRecord, 0, HeaderRecord.Length);
            //if (crc != HeaderCRC32)
            //{
                //We are not going to throw because ARJ files are just weird like that, not necessary to have a valid block :/
                //throw new InvalidFormatExceptio n("ARJ header crc mismatch");
            //}

            return crc == HeaderCRC32;
        }

        protected DateTime GetDateTime(int field)
        {
            int year = ((field >> 25) & 0x7f) + 1980;
            int month = ((field >> 21) & 0x0f);
            int day = ((field >> 16) & 0x1f);
            int hour = ((field >> 11) & 0x1f);
            int minute = ((field >> 5) & 0x3f);
            int second = ((field & 0x1f) * 2);
            //There's an issue on some dates where not even WinRAR can read the date because stuff like the minutes are 61 so we have to propagate it upwards
            if (second > 59) PropagateTime(ref second, ref minute, 60);
            if (minute > 59) PropagateTime(ref minute, ref hour, 60);
            if (hour > 23) PropagateTime(ref hour, ref day, 24);
            //Not going to do days now
            //TODO: Check Days as well although that shouldn't be necessary for now as I need to now cater for leap years as well, etc.

            return new DateTime(year, month, day, hour, minute, second);
        }

        /// <summary>
        /// Divides the datePart with the division variable, increments the nextDatePart with the results and sets the datePart to the remainder
        /// </summary>
        /// <param name="datePart">Lower Date Part</param>
        /// <param name="nextDatePart">Upper Date Part</param>
        /// <param name="division">Division amount</param>
        /// <example>PropagateTime(hour, day, 24)</example>
        private void PropagateTime(ref int datePart, ref int nextDatePart, int division)
        {
            //Doesn't work in netstandard1.0
            //var div = Math.DivRem(datePart, division, out int remainder);
            var div = datePart / division;
            nextDatePart += div;
            datePart = datePart % division;
        }
    }
}
