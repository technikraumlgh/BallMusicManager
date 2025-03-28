namespace Ametrin.Utils.WPF.FileDialogs;

public sealed record FileFilter(string Description, string Pattern)
{
    public static FileFilter AllFiles { get; } = new("All Files", "*.*");
    public override string ToString() => $"{Description} ({Pattern})|{Pattern}";

    public static FileFilter Create(string description, string pattern) => new(description, pattern);
    public static FileFilter CreateFromExtension(string description, string extension) => Create(description, extension.StartsWith('.') ? $"*{extension}" : $"*.{extension}");
}
