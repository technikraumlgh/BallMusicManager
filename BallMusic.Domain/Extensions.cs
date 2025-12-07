using System.IO;
using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;

namespace BallMusic.Domain;

public static class ZipArchiveExtensions
{
    extension(ZipArchive archive)
    {
        public ZipArchiveEntry GetOrCreateEntry(string entryName)
            => archive.GetEntry(entryName) ?? archive.CreateEntry(entryName);

        public ZipArchiveEntry OverwriteEntry(string entryName)
        {
            archive.GetEntry(entryName)?.Delete();
            return archive.CreateEntry(entryName);
        }
    }
}

public static class DirectoryInfoExtensions
{
    extension(DirectoryInfo directoryInfo)
    {
        public void CreateIfNotExists()
        {
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
        }

        public FileInfo File(string fileName) => new(Path.Combine(directoryInfo.FullName, fileName));

        public DirectoryInfo Directory(string directoryName) => new(Path.Combine(directoryInfo.FullName, directoryName));

        public void Trash(UIOption options = UIOption.OnlyErrorDialogs)
        {
            FileSystem.DeleteDirectory(directoryInfo.FullName, options, RecycleOption.SendToRecycleBin);
        }
    }

    extension(DirectoryNotFoundException)
    {
        public static DirectoryInfo ExistsOrThrow(DirectoryInfo directoryInfo)
            => directoryInfo.Exists ? directoryInfo : throw new DirectoryNotFoundException(directoryInfo.FullName);
    }

    extension(FileNotFoundException)
    {
        public static FileInfo ExistsOrThrow(FileInfo fileInfo)
            => fileInfo.Exists ? fileInfo : throw new FileNotFoundException(null, fileInfo.FullName);
    }
}
