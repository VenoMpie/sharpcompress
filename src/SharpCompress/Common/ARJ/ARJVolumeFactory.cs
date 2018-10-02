using System;
using System.Collections.Generic;
using System.IO;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Common.ARJ;
using SharpCompress.Common.ARJ.Headers;
using SharpCompress.IO;
#if !NO_FILE
using System.Linq;
using System.Text;
#endif

namespace SharpCompress.Archives.ARJ
{
    internal static class ARJVolumeFactory
    {
        internal static IEnumerable<ARJVolume> GetVolumes(StreamingMode mode, IEnumerable<Stream> streams, ReaderOptions options)
        {
            foreach (Stream s in streams)
            {
                if (!s.CanRead || !s.CanSeek)
                {
                    throw new ArgumentException("Stream is not readable and seekable");
                }
                ARJVolume volume = new ARJVolume(mode, s, options);
                yield return volume;
            }
        }

#if !NO_FILE
        internal static IEnumerable<ARJVolume> GetVolumes(StreamingMode mode, FileInfo fileInfo, ReaderOptions options)
        {
            FixOptions(options);
            ARJVolume volume = new ARJVolume(mode, fileInfo.OpenRead(), options);
            yield return volume;

            if (!volume.MainHeader.Flags.HasFlag(FileHeaderFlags.Volume))
            {
                yield break; //if file isn't volume then there is no reason to look
            }
            MainHeader ah = volume.MainHeader;
            fileInfo = GetNextFileInfo(ah, fileInfo, volume.IsFirstVolume);

            while (fileInfo != null && fileInfo.Exists)
            {
                volume = new ARJVolume(mode, fileInfo.OpenRead(), options);

                yield return volume;

                //Volumes are finished, only the first volumes have the volume flag
                if (!volume.MainHeader.Flags.HasFlag(FileHeaderFlags.Volume))
                    yield break;

                fileInfo = GetNextFileInfo(ah, fileInfo, volume.IsFirstVolume);
            }
        }
        private static ReaderOptions FixOptions(ReaderOptions options)
        {
            //make sure we're closing streams with fileinfo
            options.LeaveStreamOpen = false;
            return options;
        }

        private static FileInfo GetNextFileInfo(MainHeader mh, FileInfo fileInfo, bool isFirstVolume)
        {
            bool oldNumbering = !mh.MainFlags.HasFlag(MainHeaderExtendedFlags.AltVolumeName);
            if (oldNumbering)
            {
                return FindNextFileWithOldNumbering(fileInfo);
            }
            else
            {
                return FindNextFileWithNewNumbering(fileInfo, isFirstVolume);
            }
        }

        private static FileInfo FindNextFileWithOldNumbering(FileInfo currentFileInfo)
        {
            // .arj, .a01, .a02, ...
            string extension = currentFileInfo.Extension;

            StringBuilder buffer = new StringBuilder(currentFileInfo.FullName.Length);
            buffer.Append(currentFileInfo.FullName.Substring(0,
                                                             currentFileInfo.FullName.Length - extension.Length));
            if (string.Compare(extension, ".arj", StringComparison.OrdinalIgnoreCase) == 0)
            {
                buffer.Append(".a01");
            }
            else
            {
                int num = 1;
                if (int.TryParse(extension.Substring(2, 2), out num))
                {
                    num++;
                    buffer.Append(".a");
                    if (num < 10)
                    {
                        buffer.Append('0');
                    }
                    buffer.Append(num);
                }
                else
                {
                    ThrowInvalidFileName(currentFileInfo);
                }
            }
            return new FileInfo(buffer.ToString());
        }
        private static FileInfo FindNextFileWithNewNumbering(FileInfo currentFileInfo, bool isFirstVolume)
        {
            // *001.arj, *002.arj, ...
            string extension = currentFileInfo.Extension;
            if (string.Compare(extension, ".arj", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ArgumentException("Invalid extension, expected 'arj': " + currentFileInfo.FullName);
            }
            string FileNameWithoutExtension = Path.GetFileNameWithoutExtension(currentFileInfo.FullName);
            StringBuilder buffer = new StringBuilder(currentFileInfo.FullName.Length);
            int startIndex = FileNameWithoutExtension.Length > 3 ? FileNameWithoutExtension.Length - 3 : FileNameWithoutExtension.Length;
            buffer.Append(Path.Combine(Path.GetDirectoryName(currentFileInfo.FullName), FileNameWithoutExtension.Substring(0, startIndex)));

            string numString = isFirstVolume ? "000" : FileNameWithoutExtension.Substring(startIndex, 3);
            if (int.TryParse(numString, out int num))
            {
                num++;
                for (int i = 0; i < numString.Length - num.ToString().Length; i++)
                {
                    buffer.Append('0');
                }
                buffer.Append(num);
            }
            else
            {
                ThrowInvalidFileName(currentFileInfo);
            }
            buffer.Append(".arj");
            return new FileInfo(buffer.ToString());
        }
        private static void ThrowInvalidFileName(FileInfo fileInfo)
        {
            throw new ArgumentException("Filename invalid or next archive could not be found:"
                                        + fileInfo.FullName);
        }

#endif
    }
}