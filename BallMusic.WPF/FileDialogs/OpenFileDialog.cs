using Ametrin.Optional;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ametrin.Utils.WPF.FileDialogs;

public sealed class OpenFileDialog : IFileDialog
{
    private readonly List<FileFilter> _filters = [];
    private readonly Microsoft.Win32.OpenFileDialog _dialog = new()
    {
        CheckPathExists = true,
        DefaultExt = string.Empty,
        AddExtension = false,
        Multiselect = false,
    };

    public string Title
    {
        get => _dialog.Title;
        init => _dialog.Title = value;
    }

    public bool CheckPathExists
    {
        get => _dialog.CheckPathExists;
        init => _dialog.CheckPathExists = value;
    }

    public bool RestoreDirectory
    {
        get => _dialog.RestoreDirectory;
        init => _dialog.RestoreDirectory = value;
    }

    public string InitialDirectory
    {
        get => _dialog.InitialDirectory;
        init => _dialog.InitialDirectory = value;
    }

    public bool Multiselect
    {
        get => _dialog.Multiselect;
        init => _dialog.Multiselect = value;
    }

    public OpenFileDialog AddExtensionFilter(string description, string extension) => AddFilter(FileFilter.CreateFromExtension(description, extension));
    public OpenFileDialog AddFilter(string description, string filter) => AddFilter(FileFilter.Create(description, filter));
    public OpenFileDialog AddFilter(FileFilter filter)
    {
        _filters.Add(filter);
        return this;
    }

    public Option<FileInfo> GetFileInfo() => GetPath().Map(path => new FileInfo(path));
    public Option<string> GetPath() => ShowDialog() ? _dialog.FileName : default;

    public IEnumerable<FileInfo> GetFileInfos() => GetPaths().Select(path => new FileInfo(path));
    public IEnumerable<string> GetPaths() => ShowDialog() ? _dialog.FileNames : [];

    private bool ShowDialog()
    {
        if (_filters.Count > 0)
        {
            _dialog.Filter = string.Join('|', _filters.Select(f => f.ToString()));
        }
        return _dialog.ShowDialog() is true;
    }
}