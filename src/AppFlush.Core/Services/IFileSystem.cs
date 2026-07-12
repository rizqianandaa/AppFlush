namespace AppFlush.Core.Services;

/// <summary>Abstraksi filesystem minimal supaya LeftoverScanner bisa diuji tanpa disk asli.</summary>
public interface IFileSystem
{
    bool DirectoryExists(string path);
    IEnumerable<string> EnumerateDirectories(string path);
    void DeleteDirectory(string path);
}
