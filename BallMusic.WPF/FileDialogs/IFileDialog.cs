using Ametrin.Optional;
using System.IO;

namespace Ametrin.Utils.WPF.FileDialogs;

public interface IFileDialog
{
    public string Title { get; }
    public bool RestoreDirectory { get; }
    public string InitialDirectory { get; }

    public Option<FileInfo> GetFileInfo();
    public Option<string> GetPath();
}
