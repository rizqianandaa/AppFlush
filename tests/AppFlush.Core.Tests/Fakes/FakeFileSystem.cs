using AppFlush.Core.Services;

namespace AppFlush.Core.Tests.Fakes;

public sealed class FakeFileSystem : IFileSystem
{
    public Dictionary<string, List<string>> DirectoriesByRoot { get; } = new();
    public List<string> Deleted { get; } = new();

    public bool DirectoryExists(string path) =>
        DirectoriesByRoot.ContainsKey(path) || DirectoriesByRoot.Values.Any(list => list.Contains(path));

    public IEnumerable<string> EnumerateDirectories(string path) =>
        DirectoriesByRoot.TryGetValue(path, out var dirs) ? dirs : Enumerable.Empty<string>();

    public void DeleteDirectory(string path) => Deleted.Add(path);
}
