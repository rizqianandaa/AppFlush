namespace AppFlush.Core.Services;

/// <summary>
/// Memecah sebuah command line UninstallString registry (misalnya
/// "\"C:\Path\Uninstall.exe\" /X /uninstall") jadi (FileName, Arguments) agar bisa
/// dijalankan LANGSUNG lewat ShellExecute, tanpa perantara cmd.exe.
///
/// Ini penting: menjalankan lewat "cmd.exe /c ..." menambah satu lapis proses yang
/// tidak perlu. Beberapa installer/uninstaller vendor (termasuk yang butuh elevasi UAC,
/// atau yang memeriksa proses induknya) tidak menampilkan jendela/dialog aslinya dengan
/// benar ketika dijalankan lewat perantara seperti itu. Menjalankan file target secara
/// langsung meniru persis apa yang terjadi ketika pengguna menjalankannya sendiri lewat
/// Explorer/Control Panel, termasuk munculnya prompt UAC bila memang dibutuhkan.
/// </summary>
public static class CommandLineSplitter
{
    /// <param name="fileExists">
    /// Dipakai untuk menguji apakah sebuah potongan path benar-benar file yang ada.
    /// Defaultnya <see cref="System.IO.File.Exists(string)"/>. Tes unit menyuntikkan
    /// versi palsu supaya tidak perlu file sungguhan di disk.
    /// </param>
    public static (string FileName, string Arguments) Split(string commandLine, Func<string, bool>? fileExists = null)
    {
        fileExists ??= System.IO.File.Exists;
        var trimmed = commandLine.Trim();

        if (trimmed.StartsWith("\"", StringComparison.Ordinal))
        {
            var closingQuote = trimmed.IndexOf('"', 1);
            if (closingQuote > 0)
            {
                var fileName = trimmed.Substring(1, closingQuote - 1);
                var arguments = trimmed[(closingQuote + 1)..].Trim();
                return (fileName, arguments);
            }
        }

        // Tidak berkutip. Banyak UninstallString di registry menyimpan path TANPA
        // kutip walaupun mengandung spasi (contoh nyata: "C:\Program Files (x86)\App\
        // Uninstall.exe", tanpa argumen apa pun). Memotong asal di spasi PERTAMA akan
        // merusak path itu jadi "C:\Program" yang tidak ada. Windows sendiri
        // menyelesaikan ambiguitas ini dengan mencoba tiap potongan di setiap batas
        // spasi sampai ketemu file yang benar-benar ada -- kita tiru cara itu di sini.
        if (fileExists(trimmed))
        {
            return (trimmed, string.Empty);
        }

        var searchStart = 0;
        while (true)
        {
            var spaceIndex = trimmed.IndexOf(' ', searchStart);
            if (spaceIndex < 0) break;

            var candidate = trimmed[..spaceIndex];
            if (fileExists(candidate))
            {
                return (candidate, trimmed[(spaceIndex + 1)..].Trim());
            }
            searchStart = spaceIndex + 1;
        }

        // Tidak ada potongan yang cocok dengan file nyata di disk -- biasanya karena ini
        // nama exe pendek yang dicari lewat PATH (mis. "MsiExec.exe /X{GUID}"). Pisah di
        // spasi pertama seperti sebelumnya.
        var firstSpace = trimmed.IndexOf(' ');
        if (firstSpace < 0) return (trimmed, string.Empty);
        return (trimmed[..firstSpace], trimmed[(firstSpace + 1)..].Trim());
    }
}
