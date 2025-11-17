using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;

namespace BallMusic.Infrastructure;

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

        public Option<ZipArchiveEntry> TryGetEntry(string entryName)
            => archive.GetEntry(entryName);
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
        public static DirectoryInfo Exists(DirectoryInfo directoryInfo)
            => directoryInfo.Exists ? directoryInfo : throw new DirectoryNotFoundException(directoryInfo.FullName);
    }

    extension(FileNotFoundException)
    {
        public static FileInfo Exists(FileInfo fileInfo)
            => fileInfo.Exists ? fileInfo : throw new FileNotFoundException(fileInfo.FullName);
    }
}
