using SharpCompress.Common.ARJ.Headers;
using SharpCompress.Compressors.Rar;
using SharpCompress.IO;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpCompress.Common.ARJ.Headers
{
    internal class ARJHeaderFactory
    {
        private ReaderOptions Options { get; }
        internal StreamingMode StreamingMode { get; }

        public ARJHeaderFactory(StreamingMode mode, ReaderOptions options)
        {
            StreamingMode = mode;
            Options = options;
        }

        internal bool IsEncrypted { get; private set; }

        internal IEnumerable<ARJHeader> ReadHeaders(Stream stream)
        {
            //if (Options.LookForHeader)
            //{
            //    stream = CheckSFX(stream);
            //}

            ARJHeader header;
            while ((header = ReadNextHeader(stream)) != null)
            {
                yield return header;
            }
        }

//        private Stream CheckSFX(Stream stream)
//        {
//            RewindableStream rewindableStream = GetRewindableStream(stream);
//            stream = rewindableStream;
//            BinaryReader reader = new BinaryReader(rewindableStream);
//            try
//            {
//                int count = 0;
//                while (true)
//                {
//                    byte firstByte = reader.ReadByte();
//                    if (firstByte == 0x52)
//                    {
//                        MemoryStream buffer = new MemoryStream();
//                        byte[] nextThreeBytes = reader.ReadBytes(3);
//                        if ((nextThreeBytes[0] == 0x45)
//                            && (nextThreeBytes[1] == 0x7E)
//                            && (nextThreeBytes[2] == 0x5E))
//                        {
//                            //old format and isvalid
//                            buffer.WriteByte(0x52);
//                            buffer.Write(nextThreeBytes, 0, 3);
//                            rewindableStream.Rewind(buffer);
//                            break;
//                        }
//                        byte[] secondThreeBytes = reader.ReadBytes(3);
//                        if ((nextThreeBytes[0] == 0x61)
//                            && (nextThreeBytes[1] == 0x72)
//                            && (nextThreeBytes[2] == 0x21)
//                            && (secondThreeBytes[0] == 0x1A)
//                            && (secondThreeBytes[1] == 0x07)
//                            && (secondThreeBytes[2] == 0x00))
//                        {
//                            //new format and isvalid
//                            buffer.WriteByte(0x52);
//                            buffer.Write(nextThreeBytes, 0, 3);
//                            buffer.Write(secondThreeBytes, 0, 3);
//                            rewindableStream.Rewind(buffer);
//                            break;
//                        }
//                        buffer.Write(nextThreeBytes, 0, 3);
//                        buffer.Write(secondThreeBytes, 0, 3);
//                        rewindableStream.Rewind(buffer);
//                    }
//                    if (count > MAX_SFX_SIZE)
//                    {
//                        break;
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                if (!Options.LeaveStreamOpen)
//                {
//#if NET35
//                    reader.Close();
//#else
//                    reader.Dispose();
//#endif
//                }
//                throw new InvalidFormatException("Error trying to read rar signature.", e);
//            }
//            return stream;
//        }

        private RewindableStream GetRewindableStream(Stream stream)
        {
            RewindableStream rewindableStream = stream as RewindableStream;
            if (rewindableStream == null)
            {
                rewindableStream = new RewindableStream(stream);
            }
            return rewindableStream;
        }

        private ARJHeader ReadNextHeader(Stream stream)
        {
            var reader = new BinaryReader(stream);

            ARJHeader header = ARJHeader.Create(reader, Options.ArchiveEncoding);
            if (header == null || header.HeaderSize == 0)
            {
                return null;
            }

            switch (header.FileType)
            {
                case FileTypeEnum.MainHeader:
                    {
                        return header.PromoteHeader<MainHeader>(reader);
                    }
                case FileTypeEnum.Binary:
                case FileTypeEnum.SevenBitText:
                case FileTypeEnum.Directory:
                    {
                        FileHeader fh = header.PromoteHeader<FileHeader>(reader);
                        fh.FileStartPosition = reader.BaseStream.Position;
                        switch (StreamingMode)
                        {
                            case StreamingMode.Seekable:
                                {
                                    reader.BaseStream.Position += fh.CompressedSize;
                                }
                                break;
                            case StreamingMode.Streaming:
                                {
                                    var ms = new ReadOnlySubStream(reader.BaseStream, fh.CompressedSize);
                                    fh.PackedStream = ms;
                                }
                                break;
                            default:
                                {
                                    throw new InvalidFormatException("Invalid StreamingMode");
                                }
                        }
                        return fh;
                    }
                default:
                    {
                        return header.PromoteHeader<VolumeHeader>(reader);
                        //throw new InvalidFormatException("Invalid ARJ Header: " + header.HeaderType);
                    }
            }
        }
    }
}
