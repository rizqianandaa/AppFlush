using System.Runtime.InteropServices;

namespace AppFlush.Core.Services;

/// <summary>
/// Membaca file .lnk lewat COM "Windows Script Host Object Model" (WScript.Shell),
/// yang sudah tersedia bawaan di semua Windows -- jadi tidak perlu menulis parser
/// biner .lnk manual sendiri (format itu punya banyak kasus tepi/edge-case).
/// </summary>
public sealed class WshShortcutResolver : IShortcutResolver
{
    public string? ResolveTargetPath(string lnkPath)
    {
        if (!File.Exists(lnkPath))
            throw new ShortcutParseException($"File shortcut tidak ditemukan: {lnkPath}");

        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell")
                ?? throw new ShortcutParseException("WScript.Shell COM tidak tersedia di sistem ini.");
            dynamic shell = Activator.CreateInstance(shellType)!;
            dynamic shortcut = shell.CreateShortcut(lnkPath);
            string targetPath = shortcut.TargetPath;
            return string.IsNullOrWhiteSpace(targetPath) ? null : targetPath;
        }
        catch (COMException ex)
        {
            throw new ShortcutParseException($"Gagal membaca shortcut '{lnkPath}': {ex.Message}");
        }
    }
}
