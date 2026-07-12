using System.Text.RegularExpressions;
using AppFlush.Core.Models;

namespace AppFlush.Core.Services;

/// <summary>Mencocokkan query teks atau target shortcut (.lnk) ke daftar InstalledApp.</summary>
public static class AppMatcher
{
    /// <summary>Cari aplikasi yang namanya memuat <paramref name="query"/> (case-insensitive).</summary>
    public static IReadOnlyList<InstalledApp> FindByQuery(IEnumerable<InstalledApp> apps, string query)
    {
        var trimmed = query.Trim();
        return apps
            .Where(a => a.Name.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Cari aplikasi yang paling cocok dengan target path sebuah shortcut (.lnk).
    /// Urutan strategi: (1) target berada di dalam InstallLocation aplikasi,
    /// (2) nama file exe target (tanpa .exe) muncul di nama aplikasi.
    /// Hasil diurutkan dari paling spesifik (path match) ke paling umum (name match).
    /// </summary>
    public static IReadOnlyList<InstalledApp> FindByShortcutTarget(IEnumerable<InstalledApp> apps, string targetPath)
    {
        var appList = apps.ToList();
        if (string.IsNullOrWhiteSpace(targetPath)) return Array.Empty<InstalledApp>();

        var normalizedTarget = NormalizePath(targetPath);
        var exeStem = Path.GetFileNameWithoutExtension(targetPath);

        var pathMatches = appList
            .Where(a => !string.IsNullOrWhiteSpace(a.InstallLocation)
                        && normalizedTarget.StartsWith(NormalizePath(a.InstallLocation!), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var nameMatches = appList
            .Except(pathMatches)
            .Where(a => IsNameMatch(a.Name, exeStem))
            .ToList();

        return pathMatches.Concat(nameMatches).ToList();
    }

    private static bool IsNameMatch(string appName, string exeStem)
    {
        var normalizedAppName = NormalizeForComparison(appName);
        var normalizedExeStem = NormalizeForComparison(exeStem);
        if (normalizedExeStem.Length == 0) return false;
        return normalizedAppName.Contains(normalizedExeStem, StringComparison.OrdinalIgnoreCase)
            || normalizedExeStem.Contains(normalizedAppName, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeForComparison(string value) =>
        Regex.Replace(value, @"[^a-zA-Z0-9]", "").ToLowerInvariant();

    private static string NormalizePath(string path) =>
        path.Trim().TrimEnd('\\', '/').Replace('/', '\\');
}
