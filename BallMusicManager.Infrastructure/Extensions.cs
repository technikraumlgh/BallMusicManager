using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;



namespace BallMusicManager.Infrastructure;

public static class ZipArchiveExtensions
{
    public static ZipArchiveEntry GetOrCreateEntry(this ZipArchive archive, string entryName)
        => archive.GetEntry(entryName) ?? archive.CreateEntry(entryName);

    public static ZipArchiveEntry OverwriteEntry(this ZipArchive archive, string entryName)
    {
        archive.GetEntry(entryName)?.Delete();
        return archive.CreateEntry(entryName);
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

    public static void ForeachFile(this DirectoryInfo directoryInfo, Action<FileInfo> action, IProgress<(float, string)>? progress, SearchOption searchOption = SearchOption.AllDirectories, string pattern = "*")
    {
        if (progress is null)
        {
            directoryInfo.ForeachFile(action);
            return;
        }

        var files = directoryInfo.GetFiles(pattern, searchOption);
        float totalFiles = files.Length;
        var processed = 0;
        foreach (var file in files)
        {
            action(file);
            processed++;
            progress.Report((processed / totalFiles, file.FullName));
        }
    }

    public static void ForeachFile(this DirectoryInfo directoryInfo, Action<FileInfo> action, IProgress<float>? progress, SearchOption searchOption = SearchOption.AllDirectories, string pattern = "*")
    {
        if (progress is null)
        {
            directoryInfo.ForeachFile(action);
            return;
        }

        var files = directoryInfo.GetFiles(pattern, searchOption);
        float totalFiles = files.Length;
        var processed = 0;
        foreach (var file in files)
        {
            action(file);
            processed++;
            progress.Report(processed / totalFiles);
        }
    }

    public static void ForeachFile(this DirectoryInfo directoryInfo, Action<FileInfo> action, SearchOption searchOption = SearchOption.AllDirectories, string pattern = "*")
    {
        foreach (var file in directoryInfo.EnumerateFiles(pattern, searchOption))
        {
            action(file);
        }
    }
}
