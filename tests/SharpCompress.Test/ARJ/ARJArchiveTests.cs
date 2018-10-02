using System;
using System.IO;
using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using Xunit;
using SharpCompress.Common.ARJ.Headers;
using SharpCompress.Common.ARJ;
using SharpCompress.Archives.ARJ;
using System.Collections.Generic;

namespace SharpCompress.Test.ARJ
{
    public class ARJArchiveTests : ArchiveTests
    {
        private string ARJ_241_Stored = "ARJM0.ARJ";
        private string ARJ_241_GoodCompression = "ARJM1.ARJ";
        private string ARJ_241_LessMemoryCompression = "ARJM2.ARJ";
        private string ARJ_241_FastCompression = "ARJM3.ARJ";
        private string ARJ_241_FastestCompression = "ARJM4.ARJ";
        private string ARJ_241_Comment = "ARJCMNT.ARJ";
        private string[] ARJMVOL_241 = new string[] { "ARJMV241.ARJ", "ARJMV241.A01", "ARJMV241.A02" };
        private string[] ARJMVOL_262 = new string[] { "ARJMV262.ARJ", "ARJMV262.A01", "ARJMV262.A02" };
        private string[] ARJMVOL_286_New_Numbered = new string[] { "ARJMV286.ARJ", "ARJMV287.ARJ", "ARJMV288.ARJ" };
        private string[] ARJMVOL_286_New_Named = new string[] { "ARJMVOLN.ARJ", "ARJMV001.ARJ", "ARJMV002.ARJ" };

        public ARJArchiveTests()
        {
            UseExtensionInsteadOfNameToVerify = true;
        }

        #region Extraction
        [Fact]
        public void ARJ_241_Single_ArchiveFileRead_Extract_M0_Stored()
        {
            ArchiveFileRead("ARJM0.ARJ");
        }
        //[Fact]
        //public void ARJ_241_Single_ArchiveFileRead_Extract_M4_Fastest()
        //{
        //    ArchiveFileRead("ARJM4.ARJ");
        //}
        #endregion
        #region ARJ 2.41
        [Fact]
        public void ARJ_241_Archive_Comment()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJ_241_Comment)))
            {
                var volumes = archive.Volumes.ToList();
                Assert.Equal(1, volumes.Count);
                Assert.Equal(ARJ_241_Comment, volumes[0].MainHeader.FileName);
                Assert.Equal("TEST COMMENT\nTEST COMMENT\nTEST COMMENT\nTEST COMMENT\nTEST COMMENT\nTEST COMMENT\nTEST COMMENT\nTEST COMMENT\nTEST COMMENT\nTEST COMMEN\n", volumes[0].Comment);
            }
        }

        [Fact]
        public void ARJ_241_IsFirstVolume_True_IsMultipartVolume_True()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJMVOL_241[0])))
            {
                var volumes = archive.Volumes.ToList();
                Assert.True(volumes[0].IsMultiVolume);
                Assert.True(volumes[0].IsFirstVolume);
            }
        }

        [Fact]
        public void ARJ_241_IsFirstVolume_False_IsMultipartVolume_True()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJMVOL_241[0])))
            {
                var volumes = archive.Volumes.ToList();
                Assert.True(volumes[0].IsMultiVolume);
                Assert.False(volumes[1].IsFirstVolume);
                Assert.False(volumes[2].IsFirstVolume);
            }
        }

        [Fact]
        public void ARJ_241_Multi_ArchiveFileRead_Entries()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJMVOL_241[0])))
            {
                var entries = archive.Entries.ToList();
                Assert.Equal(3, entries.Count);
                Assert.Equal("TEST.TES", entries[0].Key);
                Assert.Equal("EXE/TEST.EXE", entries[1].Key);
                Assert.Equal("JPG/TEST.JPG", entries[2].Key);
                Assert.Equal(1, entries[0].FileHeader.Method_SecurityVersion);
                Assert.Equal(1, entries[1].FileHeader.Method_SecurityVersion);
                Assert.Equal(1, entries[2].FileHeader.Method_SecurityVersion);
                Assert.Equal("521F63FE", entries[0].Crc.ToString("X8"));
                Assert.Equal("CFB109C8", entries[1].Crc.ToString("X8"));
                Assert.Equal("088814E3", entries[2].Crc.ToString("X8"));
                Assert.Equal(999950, entries[0].CompressedSize);
                Assert.Equal(18962, entries[1].CompressedSize);
                Assert.Equal(38960, entries[2].CompressedSize);
                Assert.Equal(1000054, entries[0].Size);
                Assert.Equal(45056, entries[1].Size);
                Assert.Equal(40372, entries[2].Size);
            }
        }

        [Fact]
        public void ARJ_241_Multi_ArchiveFileRead_Volumes()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJMVOL_241[0])))
            {
                var volumes = archive.Volumes.ToList();
                Assert.Equal(3, volumes.Count);
                Assert.Equal(ARJMVOL_241[0], volumes[0].MainHeader.FileName);
                Assert.Equal(ARJMVOL_241[1], volumes[1].MainHeader.FileName);
                Assert.Equal(ARJMVOL_241[2], volumes[2].MainHeader.FileName);
            }
        }

        [Fact]
        public void ARJ_241_Multi_ArchiveStreamRead_Volumes()
        {
            using (var archive = ARJArchive.Open(ARJMVOL_241.Select(s => Path.Combine(TEST_ARCHIVES_PATH, s))
                .Select(File.OpenRead)))
            {
                var volumes = archive.Volumes.ToList();
                Assert.Equal(3, volumes.Count);
                Assert.Equal(ARJMVOL_241[0], volumes[0].MainHeader.FileName);
                Assert.Equal(ARJMVOL_241[1], volumes[1].MainHeader.FileName);
                Assert.Equal(ARJMVOL_241[2], volumes[2].MainHeader.FileName);
            }
        }

        private void Assert241SingleEntriesGeneric(List<ARJArchiveEntry> entries)
        {
            Assert.Equal(3, entries.Count);
            Assert.Equal("TEXT.TXT", entries[0].Key);
            Assert.Equal("EXE/TEST.EXE", entries[1].Key);
            Assert.Equal("JPG/TEST.JPG", entries[2].Key);
            Assert.Equal("9BD160FA", entries[0].Crc.ToString("X8"));
            Assert.Equal("CFB109C8", entries[1].Crc.ToString("X8"));
            Assert.Equal("088814E3", entries[2].Crc.ToString("X8"));
            Assert.Equal(15498, entries[0].Size);
            Assert.Equal(45056, entries[1].Size);
            Assert.Equal(40372, entries[2].Size);
        }

        [Fact]
        public void ARJ_241_Single_ArchiveFileRead_Entries_Stored()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJ_241_Stored)))
            {
                var entries = archive.Entries.ToList();
                Assert241SingleEntriesGeneric(entries);
                
                Assert.Equal(0, entries[0].FileHeader.Method_SecurityVersion);
                Assert.Equal(0, entries[1].FileHeader.Method_SecurityVersion);
                Assert.Equal(0, entries[2].FileHeader.Method_SecurityVersion);
                Assert.Equal(15498, entries[0].CompressedSize);
                Assert.Equal(45056, entries[1].CompressedSize);
                Assert.Equal(40372, entries[2].CompressedSize);
                
            }
        }

        [Fact]
        public void ARJ_241_Single_ArchiveFileRead_Entries_GoodCompression()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJ_241_GoodCompression)))
            {
                var entries = archive.Entries.ToList();
                Assert241SingleEntriesGeneric(entries);

                Assert.Equal(1, entries[0].FileHeader.Method_SecurityVersion);
                Assert.Equal(1, entries[1].FileHeader.Method_SecurityVersion);
                Assert.Equal(1, entries[2].FileHeader.Method_SecurityVersion);
                Assert.Equal(2752, entries[0].CompressedSize);
                Assert.Equal(18962, entries[1].CompressedSize);
                Assert.Equal(38930, entries[2].CompressedSize);
            }
        }

        [Fact]
        public void ARJ_241_Single_ArchiveFileRead_Entries_LessMemoryCompression()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJ_241_LessMemoryCompression)))
            {
                var entries = archive.Entries.ToList();
                Assert241SingleEntriesGeneric(entries);

                Assert.Equal(2, entries[0].FileHeader.Method_SecurityVersion);
                Assert.Equal(2, entries[1].FileHeader.Method_SecurityVersion);
                Assert.Equal(2, entries[2].FileHeader.Method_SecurityVersion);
                Assert.Equal(2758, entries[0].CompressedSize);
                Assert.Equal(18972, entries[1].CompressedSize);
                Assert.Equal(38930, entries[2].CompressedSize);
            }
        }

        [Fact]
        public void ARJ_241_Single_ArchiveFileRead_Entries_FastCompression()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJ_241_FastCompression)))
            {
                var entries = archive.Entries.ToList();
                Assert241SingleEntriesGeneric(entries);

                Assert.Equal(3, entries[0].FileHeader.Method_SecurityVersion);
                Assert.Equal(3, entries[1].FileHeader.Method_SecurityVersion);
                Assert.Equal(3, entries[2].FileHeader.Method_SecurityVersion);
                Assert.Equal(2837, entries[0].CompressedSize);
                Assert.Equal(19182, entries[1].CompressedSize);
                Assert.Equal(38955, entries[2].CompressedSize);
            }
        }

        [Fact]
        public void ARJ_241_Single_ArchiveFileRead_Entries_FastestCompression()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJ_241_FastestCompression)))
            {
                var entries = archive.Entries.ToList();
                Assert241SingleEntriesGeneric(entries);

                Assert.Equal(4, entries[0].FileHeader.Method_SecurityVersion);
                Assert.Equal(4, entries[1].FileHeader.Method_SecurityVersion);
                Assert.Equal(0, entries[2].FileHeader.Method_SecurityVersion);
                Assert.Equal(3076, entries[0].CompressedSize);
                Assert.Equal(20511, entries[1].CompressedSize);
                Assert.Equal(40372, entries[2].CompressedSize);
            }
        }

        [Fact]
        public void ARJ_241_Single_ArchiveFileRead_Volumes()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJ_241_Stored)))
            {
                var volumes = archive.Volumes.ToList();
                Assert.Equal(1, volumes.Count);
                Assert.Equal(ARJ_241_Stored, volumes[0].MainHeader.FileName);
            }
        }

        [Fact]
        public void ARJ_241_Single_ArchiveStreamRead_Volumes()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJ_241_Stored)))
            {
                var volumes = archive.Volumes.ToList();
                Assert.Equal(1, volumes.Count);
                Assert.Equal(ARJ_241_Stored, volumes[0].MainHeader.FileName);
            }
        }

        [Fact]
        public void ARJ_241_Single_ArchiveFileRead_HasDirectories()
        {
            using (var stream = File.OpenRead(Path.Combine(TEST_ARCHIVES_PATH, "ARJED.ARJ")))
            {
                using (var archive = ARJArchive.Open(stream))
                {
                    Assert.True(archive.Entries.Any(entry => entry.IsDirectory));
                }
            }
        }
        #endregion
        #region ARJ 2.62
        [Fact]
        public void ARJ_262_Multi_ArchiveFileRead_Entries()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJMVOL_262[0])))
            {
                var entries = archive.Entries.ToList();
                Assert.Equal(3, entries.Count);
                Assert.Equal("TEST.TES", entries[0].Key);
                Assert.Equal("EXE/TEST.EXE", entries[1].Key);
                Assert.Equal("JPG/TEST.JPG", entries[2].Key);
                Assert.Equal(1, entries[0].FileHeader.Method_SecurityVersion);
                Assert.Equal(1, entries[1].FileHeader.Method_SecurityVersion);
                Assert.Equal(1, entries[2].FileHeader.Method_SecurityVersion);
                Assert.Equal("521F63FE", entries[0].Crc.ToString("X8"));
                Assert.Equal("CFB109C8", entries[1].Crc.ToString("X8"));
                Assert.Equal("088814E3", entries[2].Crc.ToString("X8"));
                Assert.Equal(999950, entries[0].CompressedSize);
                Assert.Equal(18962, entries[1].CompressedSize);
                Assert.Equal(38960, entries[2].CompressedSize);
                Assert.Equal(1000054, entries[0].Size);
                Assert.Equal(45056, entries[1].Size);
                Assert.Equal(40372, entries[2].Size);
            }
        }

        [Fact]
        public void ARJ_262_Multi_ArchiveFileRead_Volumes()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJMVOL_262[0])))
            {
                var volumes = archive.Volumes.ToList();
                Assert.Equal(3, volumes.Count);
                Assert.Equal(ARJMVOL_262[0], volumes[0].MainHeader.FileName);
                Assert.Equal(ARJMVOL_262[1], volumes[1].MainHeader.FileName);
                Assert.Equal(ARJMVOL_262[2], volumes[2].MainHeader.FileName);
            }
        }

        [Fact]
        public void ARJ_262_Multi_ArchiveStreamRead_Volumes()
        {
            using (var archive = ARJArchive.Open(ARJMVOL_262.Select(s => Path.Combine(TEST_ARCHIVES_PATH, s))
                .Select(File.OpenRead)))
            {
                var volumes = archive.Volumes.ToList();
                Assert.Equal(3, volumes.Count);
                Assert.Equal(ARJMVOL_262[0], volumes[0].MainHeader.FileName);
                Assert.Equal(ARJMVOL_262[1], volumes[1].MainHeader.FileName);
                Assert.Equal(ARJMVOL_262[2], volumes[2].MainHeader.FileName);
            }
        }
        #endregion
        #region ARJ 2.86
        [Fact]
        public void ARJ_286_Multi_ArchiveFileRead_Entries()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJMVOL_286_New_Named[0])))
            {
                var entries = archive.Entries.ToList();
                Assert.Equal(3, entries.Count);
                Assert.Equal("TEST.TES", entries[0].Key);
                Assert.Equal("EXE/TEST.EXE", entries[1].Key);
                Assert.Equal("JPG/TEST.JPG", entries[2].Key);
                Assert.Equal(1, entries[0].FileHeader.Method_SecurityVersion);
                Assert.Equal(1, entries[1].FileHeader.Method_SecurityVersion);
                Assert.Equal(1, entries[2].FileHeader.Method_SecurityVersion);
                Assert.Equal("30451668", entries[0].Crc.ToString("X8"));
                Assert.Equal("CFB109C8", entries[1].Crc.ToString("X8"));
                Assert.Equal("088814E3", entries[2].Crc.ToString("X8"));
                Assert.Equal(999954, entries[0].CompressedSize);
                Assert.Equal(18962, entries[1].CompressedSize);
                Assert.Equal(38960, entries[2].CompressedSize);
                Assert.Equal(1000054, entries[0].Size);
                Assert.Equal(45056, entries[1].Size);
                Assert.Equal(40372, entries[2].Size);
            }
        }

        [Fact]
        public void ARJ_286_Multi_ArchiveFileRead_Volumes_NewNames_Named()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJMVOL_286_New_Named[0])))
            {
                var volumes = archive.Volumes.ToList();
                Assert.Equal(3, volumes.Count);
                Assert.Equal(ARJMVOL_286_New_Named[0], volumes[0].MainHeader.FileName);
                Assert.Equal(ARJMVOL_286_New_Named[1], volumes[1].MainHeader.FileName);
                Assert.Equal(ARJMVOL_286_New_Named[2], volumes[2].MainHeader.FileName);
            }
        }

        [Fact]
        public void ARJ_286_Multi_ArchiveFileRead_Volumes_NewNames_Numbered()
        {
            using (var archive = ARJArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, ARJMVOL_286_New_Numbered[0])))
            {
                var volumes = archive.Volumes.ToList();
                Assert.Equal(3, volumes.Count);
                Assert.Equal(ARJMVOL_286_New_Numbered[0], volumes[0].MainHeader.FileName);
                Assert.Equal(ARJMVOL_286_New_Numbered[1], volumes[1].MainHeader.FileName);
                Assert.Equal(ARJMVOL_286_New_Numbered[2], volumes[2].MainHeader.FileName);
            }
        }

        [Fact]
        public void ARJ_286_Multi_ArchiveStreamRead_Volumes()
        {
            using (var archive = ARJArchive.Open(ARJMVOL_286_New_Named.Select(s => Path.Combine(TEST_ARCHIVES_PATH, s))
                .Select(File.OpenRead)))
            {
                var volumes = archive.Volumes.ToList();
                Assert.Equal(3, volumes.Count);
                Assert.Equal(ARJMVOL_286_New_Named[0], volumes[0].MainHeader.FileName);
                Assert.Equal(ARJMVOL_286_New_Named[1], volumes[1].MainHeader.FileName);
                Assert.Equal(ARJMVOL_286_New_Named[2], volumes[2].MainHeader.FileName);
            }
        }
        #endregion
    }
}
