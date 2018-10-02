using System.Collections.Generic;
using SharpCompress.Common;
using SharpCompress.Common.ARJ.Headers;
using SharpCompress.Common.ARJ;
using SharpCompress.Archives.ARJ;
using System.Linq;

namespace SharpCompress.Archives.ARJ
{
    internal static class ARJEntryFactory
    {
        private static IEnumerable<ARJFilePart> GetFileParts(IEnumerable<ARJVolume> volumes)
        {
            foreach (ARJVolume arjVolume in volumes)
            {
                foreach (ARJFilePart fp in arjVolume.GetVolumeFileParts())
                {
                    yield return fp;
                }
            }
        }

        private static IEnumerable<IEnumerable<ARJFilePart>> GetGroupedFileParts(IEnumerable<ARJVolume> volumes)
        {
            var groups = GetFileParts(volumes).ToList().GroupBy(a => a.FilePartName);
            foreach (var group in groups)
            {
                yield return group.ToList();
            }
        }

        internal static IEnumerable<ARJArchiveEntry> GetEntries(ARJArchive archive, IEnumerable<ARJVolume> volumes)
        {
            foreach (var groupedParts in GetGroupedFileParts(volumes))
            {
                yield return new ARJArchiveEntry(archive, groupedParts);
            }
        }
    }
}