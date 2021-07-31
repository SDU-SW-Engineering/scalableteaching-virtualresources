using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace ScalableTeaching.Data
{
    public class ZipBuilder
    {
        public static async Task<InMemoryFile> BuildZip(List<InMemoryFile> files, string outputFileName)
        {
            var zip = new InMemoryFile() { FileName = outputFileName };
            using (var archiveStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
                {
                    foreach (var file in files)
                    {
                        var entry = archive.CreateEntry(file.FileName, CompressionLevel.Optimal);
                        await using var zipStream = entry.Open();
                        await zipStream.WriteAsync(file.Content.AsMemory(0, file.Content.Length));
                    }
                }
                zip.Content = archiveStream.ToArray();
            }
            return zip;
        }

        public static async Task<InMemoryFile> BuildZip(InMemoryFile file, string outputFileName)
        {
            return await BuildZip(new List<InMemoryFile> { file }, outputFileName);
        }
    }
}