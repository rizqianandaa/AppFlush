using AppFlush.Core.Models;
using AppFlush.Core.RegistryAccess;

namespace AppFlush.Core.Services;

/// <summary>Mengubah entri registry mentah jadi daftar InstalledApp yang bersih.</summary>
public static class RegistryScanner
{
    public static IReadOnlyList<InstalledApp> ScanInstalledApps(
        IRegistryBackend backend, bool includeSystemComponents = false)
    {
        var apps = new List<InstalledApp>();

        foreach (var entry in backend.EnumerateUninstallEntries())
        {
            if (!entry.TryGetValue("DisplayName", out var name) || string.IsNullOrWhiteSpace(name))
                continue;

            if (!includeSystemComponents
                && entry.TryGetValue("SystemComponent", out var systemComponent)
                && systemComponent == "1")
                continue;

            apps.Add(new InstalledApp
            {
                Name = name,
                Version = entry.GetValueOrDefault("DisplayVersion"),
                Publisher = entry.GetValueOrDefault("Publisher"),
                UninstallString = entry.GetValueOrDefault("UninstallString"),
                InstallLocation = entry.GetValueOrDefault("InstallLocation"),
                IsMsi = entry.GetValueOrDefault("WindowsInstaller") == "1",
                RegistryKeyPath = entry.GetValueOrDefault("__KeyPath__") ?? "",
            });
        }

        return apps
            .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
