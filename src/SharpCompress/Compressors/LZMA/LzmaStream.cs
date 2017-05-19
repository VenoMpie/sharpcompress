﻿using System;
using System.IO;
using SharpCompress.Compressors.LZMA.LZ;
using SharpCompress.Converters;

namespace SharpCompress.Compressors.LZMA
{
    public class LzmaStream : Stream
    {
        private readonly Stream inputStream;
        private readonly long inputSize;
        private readonly long outputSize;

        private readonly int dictionarySize;
        private readonly OutWindow outWindow = new OutWindow();
        private readonly RangeCoder.Decoder rangeDecoder = new RangeCoder.Decoder();
        private Decoder decoder;

        private long position;
        private bool endReached;
        private long availableBytes;
        private long rangeDecoderLimit;
        private long inputPosition;

        // LZMA2
        private readonly bool isLZMA2;
        private bool uncompressedChunk;
        private bool needDictReset = true;
        private bool needProps = true;

        private readonly Encoder encoder;
        private bool isDisposed;

        public LzmaStream(byte[] properties, Stream inputStream)
            : this(properties, inputStream, -1, -1, null, properties.Length < 5)
        {
        }

        public LzmaStream(byte[] properties, Stream inputStream, long inputSize)
            : this(properties, inputStream, inputSize, -1, null, properties.Length < 5)
        {
        }

        public LzmaStream(byte[] properties, Stream inputStream, long inputSize, long outputSize)
            : this(properties, inputStream, inputSize, outputSize, null, properties.Length < 5)
        {
        }

        public LzmaStream(byte[] properties, Stream inputStream, long inputSize, long outputSize,
                          Stream presetDictionary, bool isLZMA2)
        {
            this.inputStream = inputStream;
            this.inputSize = inputSize;
            this.outputSize = outputSize;
            this.isLZMA2 = isLZMA2;

            if (!isLZMA2)
            {
                dictionarySize = DataConverter.LittleEndian.GetInt32(properties, 1);
                outWindow.Create(dictionarySize);
                if (presetDictionary != null)
                {
                    outWindow.Train(presetDictionary);
                }

                rangeDecoder.Init(inputStream);

                decoder = new Decoder();
                decoder.SetDecoderProperties(properties);
                Properties = properties;

                availableBytes = outputSize < 0 ? long.MaxValue : outputSize;
                rangeDecoderLimit = inputSize;
            }
            else
            {
                dictionarySize = 2 | (properties[0] & 1);
                dictionarySize <<= (properties[0] >> 1) + 11;

                outWindow.Create(dictionarySize);
                if (presetDictionary != null)
                {
                    outWindow.Train(presetDictionary);
                    needDictReset = false;
                }

                Properties = new byte[1];
                availableBytes = 0;
            }
        }

        public LzmaStream(LzmaEncoderProperties properties, bool isLZMA2, Stream outputStream)
            : this(properties, isLZMA2, null, outputStream)
        {
        }

        public LzmaStream(LzmaEncoderProperties properties, bool isLZMA2, Stream presetDictionary, Stream outputStream)
        {
            this.isLZMA2 = isLZMA2;
            availableBytes = 0;
            endReached = true;

            if (isLZMA2)
            {
                throw new NotImplementedException();
            }

            encoder = new Encoder();
            encoder.SetCoderProperties(properties.propIDs, properties.properties);
            MemoryStream propStream = new MemoryStream(5);
            encoder.WriteCoderProperties(propStream);
            Properties = propStream.ToArray();

            encoder.SetStreams(null, outputStream, -1, -1);
            if (presetDictionary != null)
            {
                encoder.Train(presetDictionary);
            }
        }

        public override bool CanRead => encoder == null;

        public override bool CanSeek => false;

        public override bool CanWrite => encoder != null;

        public override void Flush()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;
            if (disposing)
            {
                if (encoder != null)
                {
                    position = encoder.Code(null, true);
                }
                if (inputStream != null)
                {
                    inputStream.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        public override long Length => position + availableBytes;

        public override long Position { get => position; set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (endReached)
            {
                return 0;
            }

            int total = 0;
            while (total < count)
            {
                if (availableBytes == 0)
                {
                    if (isLZMA2)
                    {
                        decodeChunkHeader();
                    }
                    else
                    {
                        endReached = true;
                    }
                    if (endReached)
                    {
                        break;
                    }
                }

                int toProcess = count - total;
                if (toProcess > availableBytes)
                {
                    toProcess = (int)availableBytes;
                }

                outWindow.SetLimit(toProcess);
                if (uncompressedChunk)
                {
                    inputPosition += outWindow.CopyStream(inputStream, toProcess);
                }
                else if (decoder.Code(dictionarySize, outWindow, rangeDecoder)
                         && outputSize < 0)
                {
                    availableBytes = outWindow.AvailableBytes;
                }

                int read = outWindow.Read(buffer, offset, toProcess);
                total += read;
                offset += read;
                position += read;
                availableBytes -= read;

                if (availableBytes == 0 && !uncompressedChunk)
                {
                    rangeDecoder.ReleaseStream();
                    if (!rangeDecoder.IsFinished || (rangeDecoderLimit >= 0 && rangeDecoder.Total != rangeDecoderLimit))
                    {
                        throw new DataErrorException();
                    }
                    inputPosition += rangeDecoder.Total;
                    if (outWindow.HasPending)
                    {
                        throw new DataErrorException();
                    }
                }
            }

            if (endReached)
            {
                if (inputSize >= 0 && inputPosition != inputSize)
                {
                    throw new DataErrorException();
                }
                if (outputSize >= 0 && position != outputSize)
                {
                    throw new DataErrorException();
                }
            }

            return total;
        }

        private void decodeChunkHeader()
        {
            int control = inputStream.ReadByte();
            inputPosition++;

            if (control == 0x00)
            {
                endReached = true;
                return;
            }

            if (control >= 0xE0 || control == 0x01)
            {
                needProps = true;
                needDictReset = false;
                outWindow.Reset();
            }
            else if (needDictReset)
            {
                throw new DataErrorException();
            }

            if (control >= 0x80)
            {
                uncompressedChunk = false;

                availableBytes = (control & 0x1F) << 16;
                availableBytes += (inputStream.ReadByte() << 8) + inputStream.ReadByte() + 1;
                inputPosition += 2;

                rangeDecoderLimit = (inputStream.ReadByte() << 8) + inputStream.ReadByte() + 1;
                inputPosition += 2;

                if (control >= 0xC0)
                {
                    needProps = false;
                    Properties[0] = (byte)inputStream.ReadByte();
                    inputPosition++;

                    decoder = new Decoder();
                    decoder.SetDecoderProperties(Properties);
                }
                else if (needProps)
                {
                    throw new DataErrorException();
                }
                else if (control >= 0xA0)
                {
                    decoder = new Decoder();
                    decoder.SetDecoderProperties(Properties);
                }

                rangeDecoder.Init(inputStream);
            }
            else if (control > 0x02)
            {
                throw new DataErrorException();
            }
            else
            {
                uncompressedChunk = true;
                availableBytes = (inputStream.ReadByte() << 8) + inputStream.ReadByte() + 1;
                inputPosition += 2;
            }
        }

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
            if (encoder != null)
            {
                position = encoder.Code(new MemoryStream(buffer, offset, count), false);
            }
        }

        public byte[] Properties { get; } = new byte[5];
    }
}