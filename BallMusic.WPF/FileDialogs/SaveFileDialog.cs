using Ametrin.Optional;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ametrin.Utils.WPF.FileDialogs;

public sealed class SaveFileDialog : IFileDialog
{
    private readonly List<FileFilter> _filters = [];
    private readonly Microsoft.Win32.SaveFileDialog _dialog = new()
    {
        CheckFileExists = false,
        DefaultExt = string.Empty,
        AddExtension = false,
    };

    public string Title
    {
        get => _dialog.Title;
        init => _dialog.Title = value;
    }

    public bool RestoreDirectory
    {
        get => _dialog.RestoreDirectory;
        init => _dialog.RestoreDirectory = value;
    }

    public string DefaultExtension
    {
        get => _dialog.DefaultExt;
        init
        {
            var isEmpty = string.IsNullOrWhiteSpace(value);
            _dialog.DefaultExt = isEmpty ? string.Empty : value; //prevent null and " " from getting through
            _dialog.AddExtension = !isEmpty;
        }
    }

    public string InitialDirectory
    {
        get => _dialog.InitialDirectory;
        init => _dialog.InitialDirectory = value;
    }

    public SaveFileDialog AddExtensionFilter(string description, string extension) => AddFilter(FileFilter.CreateFromExtension(description, extension));
    public SaveFileDialog AddFilter(string description, string pattern) => AddFilter(FileFilter.Create(description, pattern));
    public SaveFileDialog AddFilter(FileFilter filter)
    {
        _filters.Add(filter);
        return this;
    }

    public Option<FileInfo> GetFileInfo() => GetPath().Map(path => new FileInfo(path));
    public Option<string> GetPath() => ShowDialog() ? _dialog.FileName : default;

    private bool ShowDialog()
    {
        if (_filters.Count > 0)
        {
            _dialog.Filter = string.Join('|', _filters.Select(f => f.ToString()));
        }
        return _dialog.ShowDialog() is true;
    }
}