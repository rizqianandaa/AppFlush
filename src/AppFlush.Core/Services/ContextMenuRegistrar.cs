using AppFlush.Core.RegistryAccess;

namespace AppFlush.Core.Services;

/// <summary>
/// Daftar/hapus menu klik-kanan "Uninstall dengan AppFlush" untuk file .lnk.
/// Ditulis ke HKEY_CURRENT_USER supaya pendaftarannya sendiri tidak perlu hak admin.
///
/// Catatan penting soal Windows 11: ini adalah verb shell KLASIK (lewat registry
/// biasa), bukan ekstensi context-menu MSIX/Sparse Package. Akibatnya, item ini
/// TIDAK akan pernah tampil di level teratas menu klik-kanan baru Windows 11 --
/// selalu masuk ke submenu "Show more options" (yang membuka menu klik-kanan gaya
/// lama). Ini perilaku Windows 11 untuk SEMUA verb shell klasik pihak ketiga, bukan
/// bug khusus AppFlush. Satu-satunya cara resmi untuk tampil di level teratas adalah
/// membungkus ulang aplikasi sebagai paket MSIX dengan ekstensi context menu dari
/// Windows App SDK -- jauh lebih kompleks dari installer .exe biasa yang dipakai
/// sekarang, dan di luar cakupan perubahan ini.
/// </summary>
public static class ContextMenuRegistrar
{
    public const string MenuLabel = "Uninstall dengan AppFlush";
    private const string ContextMenuKeyPath = @"Software\Classes\lnkfile\shell\AppFlush";
    private const string CommandKeyPath = ContextMenuKeyPath + @"\command";

    public static string BuildCommand(string exePath) =>
        $"\"{exePath}\" from-shortcut \"%1\"";

    public static void Register(IRegistryBackend backend, string command, string label = MenuLabel)
    {
        backend.SetValue(RegistryHive.CurrentUser, ContextMenuKeyPath, "", label);
        backend.SetValue(RegistryHive.CurrentUser, CommandKeyPath, "", command);
    }

    public static void Unregister(IRegistryBackend backend) =>
        backend.DeleteTree(RegistryHive.CurrentUser, ContextMenuKeyPath);

    public static bool IsRegistered(IRegistryBackend backend) =>
        backend.ReadValues(RegistryHive.CurrentUser, CommandKeyPath) is not null;
}
