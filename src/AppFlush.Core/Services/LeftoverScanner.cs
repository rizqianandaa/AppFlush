using System.Text.RegularExpressions;
using AppFlush.Core.Models;

namespace AppFlush.Core.Services;

/// <summary>Mencari folder yang kemungkinan sisa dari aplikasi yang sudah diuninstall.</summary>
public static class LeftoverScanner
{
    public static IReadOnlyList<string> DefaultSearchRoots()
    {
        var roots = new List<string>
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        };
        return roots.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
    }

    public static IReadOnlyList<LeftoverCandidate> FindLeftoverDirs(
        InstalledApp app, IFileSystem fileSystem, IEnumerable<string>? searchRoots = null)
    {
        searchRoots ??= DefaultSearchRoots();
        var candidates = new List<LeftoverCandidate>();
        var nameKey = NormalizeName(app.Name);
        var publisherKey = string.IsNullOrWhiteSpace(app.Publisher) ? null : NormalizeName(app.Publisher);

        if (nameKey.Length == 0) return candidates;

        foreach (var root in searchRoots)
        {
            if (!fileSystem.DirectoryExists(root)) continue;

            foreach (var dir in fileSystem.EnumerateDirectories(root))
            {
                var dirName = Path.GetFileName(dir.TrimEnd('\\', '/'));
                var normalizedDirName = NormalizeName(dirName);
                if (normalizedDirName.Length == 0) continue;

                if (normalizedDirName.Contains(nameKey) || nameKey.Contains(normalizedDirName))
                {
                    candidates.Add(new LeftoverCandidate(dir, "Nama folder cocok dengan nama aplikasi '" + app.Name + "'"));
                }
                else if (publisherKey is not null && normalizedDirName.Contains(publisherKey))
                {
                    candidates.Add(new LeftoverCandidate(dir, "Nama folder cocok dengan publisher '" + app.Publisher + "'"));
                }
            }
        }

        return candidates;
    }

    public static IReadOnlyList<string> DeleteCandidates(IFileSystem fileSystem, IEnumerable<string> paths)
    {
        var deleted = new List<string>();
        foreach (var path in paths)
        {
            if (!fileSystem.DirectoryExists(path)) continue;
            fileSystem.DeleteDirectory(path);
            deleted.Add(path);
        }
        return deleted;
    }

    private static string NormalizeName(string value) =>
        Regex.Replace(value, @"[^a-zA-Z0-9]", "").ToLowerInvariant();
}
