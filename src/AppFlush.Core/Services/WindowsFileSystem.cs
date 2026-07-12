namespace AppFlush.Core.Services;

public sealed class WindowsFileSystem : IFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public IEnumerable<string> EnumerateDirectories(string path) =>
        Directory.Exists(path) ? Directory.EnumerateDirectories(path) : Enumerable.Empty<string>();

    public void DeleteDirectory(string path) => Directory.Delete(path, recursive: true);
}
