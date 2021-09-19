using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace ScalableTeaching.Data
{
    public class InMemoryFile
    {
        public InMemoryFile(string fileName, byte[] content)
        {
            FileName = fileName;
            Content = content;
        }

        public InMemoryFile()
        {
        }

        public string FileName { get; set; }
        public byte[] Content { get; set; }

        public FileStreamResult ToFileStreamResult()
        {
            return new FileStreamResult(new MemoryStream(Content), "application/octet-stream")
            {
                FileDownloadName = FileName
            };
        }
    }
}