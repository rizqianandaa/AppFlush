using System.Diagnostics;
using System.IO;

namespace AppFlush.Core.Services;

/// <summary>
/// Menjalankan command line uninstall LANGSUNG lewat ShellExecute pada file targetnya
/// (bukan lewat perantara cmd.exe /c).
///
/// PENTING -- perbaikan dari versi sebelumnya: kode lama menjalankan setiap uninstaller
/// lewat "cmd.exe /c <command>". Itu menambah satu proses perantara yang tidak perlu.
/// Beberapa uninstaller vendor (termasuk punya Zoom, yang dijalankan dari AppData dan
/// bisa saja butuh elevasi UAC untuk membersihkan entri di HKLM/Program Files) tidak
/// tampil dengan benar ketika dijalankan lewat cmd.exe sebagai perantara: prompt UAC atau
/// jendela uninstaller vendor bisa gagal muncul sama sekali, membuat AppFlush terlihat
/// "macet" menunggu proses yang sebenarnya sudah gagal start secara diam-diam.
///
/// Dengan memanggil ShellExecute langsung pada file .exe target (sama seperti ketika
/// pengguna menjalankannya sendiri lewat Explorer/Control Panel), Windows menangani UAC
/// dan pembuatan jendela GUI-nya secara normal.
/// </summary>
public sealed class WindowsProcessRunner : IProcessRunner
{
    public int Run(string commandLine)
    {
        var (fileName, arguments) = CommandLineSplitter.Split(commandLine);

        if (!File.Exists(fileName))
        {
            // Sebelumnya kalau file tidak ada, Process.Start dengan UseShellExecute=true
            // masih "berhasil" memanggil ShellExecuteEx yang bisa diam-diam gagal tanpa
            // exception yang jelas untuk pengguna (persis gejala "tidak terjadi apa-apa").
            // Sekarang kita cek dulu secara eksplisit dan lempar pesan yang jelas.
            throw new UninstallException(
                $"File uninstaller tidak ditemukan: '{fileName}'.\nPerintah asli: {commandLine}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true,
            // PENTING (fix bug sebelumnya): tanpa Verb="runas", Windows hanya meminta
            // elevasi UAC untuk exe yang namanya cocok dengan heuristik "Installer Detection
            // Technology" (biasanya cuma exe tak bermanifes bernama setup/install/update).
            // Banyak uninstaller vendor (AnyDesk.exe, Update.exe, dll) TIDAK cocok heuristik
            // itu, jadi dijalankan tanpa hak admin -- lalu diam-diam berhenti sendiri begitu
            // uninstaller itu sadar tidak punya izin tulis ke Program Files/registry HKLM,
            // tanpa menunjukkan jendela apa pun. Itu sebabnya terlihat "tidak terjadi apa-apa".
            // Verb="runas" MEMAKSA Windows menampilkan prompt UAC asli setiap kali uninstall
            // dijalankan, sama seperti tombol "Uninstall" di Programs and Features bawaan
            // Windows.
            Verb = "runas",
        };

        Process process;
        try
        {
            process = Process.Start(startInfo)
                ?? throw new UninstallException($"Gagal memulai proses uninstall untuk '{fileName}'. Pastikan file tersebut masih ada.");
        }
        catch (System.ComponentModel.Win32Exception win32Ex) when (win32Ex.NativeErrorCode == 1223)
        {
            // ERROR_CANCELLED: pengguna klik "No" di prompt UAC.
            throw new UninstallException("Uninstall dibatalkan karena permintaan izin administrator (UAC) ditolak.");
        }

        using (process)
        {
            process.WaitForExit();
            return process.ExitCode;
        }
    }
}
