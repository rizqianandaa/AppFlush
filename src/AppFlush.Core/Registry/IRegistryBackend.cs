namespace AppFlush.Core.RegistryAccess;

/// <summary>
/// Abstraksi atas Windows Registry supaya logika bisnis (RegistryScanner,
/// ContextMenuRegistrar, dst) bisa diuji tanpa menyentuh registry sungguhan.
/// Implementasi nyata: <see cref="WindowsRegistryBackend"/>. Implementasi palsu
/// untuk unit test ada di proyek tes (Fakes/FakeRegistryBackend.cs).
/// </summary>
public interface IRegistryBackend
{
    /// <summary>
    /// Mengembalikan semua entri di bawah key "...\Uninstall" (HKLM 64-bit,
    /// HKLM WOW6432Node, dan HKCU), masing-masing sebagai dictionary nama-nilai
    /// (DisplayName, DisplayVersion, Publisher, UninstallString, InstallLocation,
    /// SystemComponent, WindowsInstaller) plus kunci internal "__KeyPath__".
    /// </summary>
    IEnumerable<IReadOnlyDictionary<string, string>> EnumerateUninstallEntries();

    /// <summary>Baca semua value di satu key tertentu. Null kalau key tidak ada.</summary>
    IReadOnlyDictionary<string, string>? ReadValues(RegistryHive hive, string subKeyPath);

    /// <summary>Tulis satu value (membuat key-nya kalau belum ada).</summary>
    void SetValue(RegistryHive hive, string subKeyPath, string valueName, string value);

    /// <summary>Hapus satu key beserta seluruh sub-key di bawahnya.</summary>
    void DeleteTree(RegistryHive hive, string subKeyPath);
}
