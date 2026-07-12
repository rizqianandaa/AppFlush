namespace AppFlush.Core.Services;

public sealed class ShortcutParseException : Exception
{
    public ShortcutParseException(string message) : base(message) { }
}

/// <summary>Membaca target path (.exe) dari sebuah file shortcut (.lnk).</summary>
public interface IShortcutResolver
{
    string? ResolveTargetPath(string lnkPath);
}
