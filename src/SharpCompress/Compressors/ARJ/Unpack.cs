using System;
using System.Collections.Generic;
using System.IO;
using SharpCompress.Common;
using SharpCompress.Common.ARJ.Headers;
//using SharpCompress.Compressors.ARJ.Decode;

namespace SharpCompress.Compressors.ARJ
{
    internal sealed class Unpack
    {
        public bool FileExtracted { get; private set; }

        internal long destUnpSize;
        internal bool suspended;

        public long DestSize
        {
            get => destUnpSize;
            set
            {
                destUnpSize = value;
                FileExtracted = false;
            }
        }

        public bool Suspended { set => suspended = value; }

        private FileHeader fileHeader;
        private Stream stream;

        public Unpack(FileHeader fileHeader, Stream readStream)
        {
            this.fileHeader = fileHeader;
            this.stream = readStream;
            destUnpSize = fileHeader.OriginalSize;
            DoUnpack();
        }

        public void DoUnpack()
        {
            switch (fileHeader.Method_SecurityVersion)
            {
                case 0:
                    Unstore();
                    break;
                case 1:
                case 2:
                case 3:
                    UnpackCompressed();
                    break;
                case 4: //Fastest
                    UnpackFastest();
                    break;
            }
        }

        private void Unstore()
        {
            byte[] buffer = new byte[0x10000];
            while (true)
            {
                int code = stream.Read(buffer, 0, (int)Math.Min(buffer.Length, destUnpSize));
                if (code == 0 || code == -1)
                {
                    break;
                }
                code = code < destUnpSize ? code : (int)destUnpSize;
                //TODO: ARJ -> Finish This
                //writeStream.Write(buffer, 0, code);
                if (destUnpSize >= 0)
                {
                    destUnpSize -= code;
                }
                if (suspended)
                {
                    return;
                }
            }
        }

        private void UnpackCompressed()
        {
            throw new NotSupportedException();
        }

        private void UnpackFastest()
        {
            throw new NotSupportedException();
        }
    }
}