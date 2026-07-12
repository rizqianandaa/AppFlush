using Microsoft.Win32;

namespace AppFlush.Core.RegistryAccess;

/// <summary>
/// Implementasi nyata IRegistryBackend memakai Microsoft.Win32.Registry.
/// Hanya berjalan di Windows. Sengaja dibuat tipis (tanpa logika bisnis)
/// supaya semua logika penting (filter/parsing) tetap ada di kelas yang
/// diuji lewat FakeRegistryBackend.
/// </summary>
public sealed class WindowsRegistryBackend : IRegistryBackend
{
    private static readonly string[] UninstallRoots =
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
    };

    private static readonly string[] ValueNames =
    {
        "DisplayName", "DisplayVersion", "Publisher", "UninstallString",
        "InstallLocation", "SystemComponent", "WindowsInstaller",
    };

    public IEnumerable<IReadOnlyDictionary<string, string>> EnumerateUninstallEntries()
    {
        foreach (var (baseKey, hiveName) in new[] { (Registry.LocalMachine, "HKLM"), (Registry.CurrentUser, "HKCU") })
        {
            foreach (var root in UninstallRoots)
            {
                using var uninstallKey = baseKey.OpenSubKey(root);
                if (uninstallKey is null) continue;

                foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                {
                    using var appKey = uninstallKey.OpenSubKey(subKeyName);
                    if (appKey is null) continue;

                    var entry = new Dictionary<string, string>();
                    foreach (var valueName in ValueNames)
                    {
                        var value = appKey.GetValue(valueName);
                        if (value is not null)
                            entry[valueName] = value.ToString() ?? "";
                    }
                    entry["__KeyPath__"] = $"{hiveName}\\{root}\\{subKeyName}";
                    yield return entry;
                }
            }
        }
    }

    public IReadOnlyDictionary<string, string>? ReadValues(RegistryHive hive, string subKeyPath)
    {
        using var key = OpenBase(hive).OpenSubKey(subKeyPath);
        if (key is null) return null;
        var result = new Dictionary<string, string>();
        foreach (var valueName in key.GetValueNames())
        {
            result[valueName] = key.GetValue(valueName)?.ToString() ?? "";
        }
        return result;
    }

    public void SetValue(RegistryHive hive, string subKeyPath, string valueName, string value)
    {
        using var key = OpenBase(hive).CreateSubKey(subKeyPath, writable: true);
        key.SetValue(valueName, value);
    }

    public void DeleteTree(RegistryHive hive, string subKeyPath)
    {
        OpenBase(hive).DeleteSubKeyTree(subKeyPath, throwOnMissingSubKey: false);
    }

    private static RegistryKey OpenBase(RegistryHive hive) => hive switch
    {
        RegistryHive.CurrentUser => Registry.CurrentUser,
        RegistryHive.LocalMachine => Registry.LocalMachine,
        _ => throw new ArgumentOutOfRangeException(nameof(hive)),
    };
}
