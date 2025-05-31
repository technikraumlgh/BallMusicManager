using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;



namespace BallMusic.Infrastructure;

public static class ZipArchiveExtensions
{
    public static ZipArchiveEntry GetOrCreateEntry(this ZipArchive archive, string entryName)
        => archive.GetEntry(entryName) ?? archive.CreateEntry(entryName);

    public static ZipArchiveEntry OverwriteEntry(this ZipArchive archive, string entryName)
    {
        archive.GetEntry(entryName)?.Delete();
        return archive.CreateEntry(entryName);
    }

    public static Option<ZipArchiveEntry> TryGetEntry(this ZipArchive archive, string entryName)
    {
        return archive.GetEntry(entryName);
    }
}

public static class DirectoryInfoExtensions
{
    public static DirectoryInfo GetCopyOfPathIfExists(this DirectoryInfo directoryInfo)
    {
        return directoryInfo.Exists ? directoryInfo.GetCopyOfPath().GetCopyOfPathIfExists() : directoryInfo;
    }

    public static DirectoryInfo GetCopyOfPath(this DirectoryInfo directoryInfo)
    {
        return new(directoryInfo.FullName + " - Copy");
    }

    public static void CreateIfNotExists(this DirectoryInfo directoryInfo)
    {
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }
    }

    public static FileInfo File(this DirectoryInfo directoryInfo, string fileName) => new(Path.Combine(directoryInfo.FullName, fileName));
    public static DirectoryInfo Directory(this DirectoryInfo directoryInfo, string directoryName) => new(Path.Combine(directoryInfo.FullName, directoryName));

    public static void Trash(this DirectoryInfo info, UIOption options = UIOption.OnlyErrorDialogs)
    {
        FileSystem.DeleteDirectory(info.FullName, options, RecycleOption.SendToRecycleBin);
    }
}
