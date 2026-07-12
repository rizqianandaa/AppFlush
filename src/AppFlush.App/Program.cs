using AppFlush.Core.RegistryAccess;
using AppFlush.Core.Services;
using AppFlush.Core.Workflows;

namespace AppFlush.App;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        // PENTING: tanpa ini, exception yang tidak tertangkap di mana pun bisa
        // membuat AppFlush diam-diam gagal/tertutup tanpa pesan apa pun -- persis
        // gejala "tidak kelihatan terjadi apa-apa" yang sulit dibedakan dari
        // command yang memang berjalan sukses tapi diam (silent). Dengan handler
        // ini, setiap error tak terduga SELALU akan muncul lewat kotak dialog.
        Application.ThreadException += (_, e) => ShowUnexpectedError(e.Exception);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString() ?? "Unknown error");
            ShowUnexpectedError(ex);
        };

        var backend = new WindowsRegistryBackend();

        if (args.Length >= 2 && string.Equals(args[0], "from-shortcut", StringComparison.OrdinalIgnoreCase))
        {
            RunFromShortcut(args[1], backend);
            return;
        }

        if (args.Length >= 1 && string.Equals(args[0], "register-menu", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var command = ContextMenuRegistrar.BuildCommand(Application.ExecutablePath);
                ContextMenuRegistrar.Register(backend, command);
            }
            catch (Exception ex)
            {
                ShowUnexpectedError(ex);
            }
            return;
        }

        if (args.Length >= 1 && string.Equals(args[0], "unregister-menu", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                ContextMenuRegistrar.Unregister(backend);
            }
            catch (Exception ex)
            {
                ShowUnexpectedError(ex);
            }
            return;
        }

        Application.Run(new MainForm(backend, new WindowsFileSystem()));
    }

    private static void ShowUnexpectedError(Exception ex)
    {
        MessageBox.Show(
            $"Terjadi error yang tidak terduga:\n\n{ex.Message}\n\n{ex.GetType().Name}",
            "AppFlush - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>
    /// Dipanggil oleh menu klik-kanan "Uninstall dengan AppFlush" pada file .lnk.
    /// Sengaja TIDAK menampilkan dialog konfirmasi buatan AppFlush sendiri lagi --
    /// begitu diklik dari menu klik-kanan, uninstaller resmi aplikasi tersebut
    /// langsung dijalankan (uninstaller itu sendiri yang biasanya menampilkan
    /// konfirmasinya). Tidak ada jendela console yang bisa "muncul sejenak lalu
    /// hilang" karena tetap lewat proses GUI (WindowsProcessRunner), bukan console.
    /// </summary>
    private static void RunFromShortcut(string lnkPath, IRegistryBackend backend)
    {
        var resolver = new WshShortcutResolver();
        var plan = ShortcutUninstallWorkflow.Prepare(lnkPath, resolver, backend);

        switch (plan.Outcome)
        {
            case ShortcutUninstallOutcome.ShortcutUnreadable:
            case ShortcutUninstallOutcome.NoTargetPath:
            case ShortcutUninstallOutcome.NoMatchingApp:
            case ShortcutUninstallOutcome.CommandUnavailable:
                MessageBox.Show(plan.Message, "AppFlush", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            case ShortcutUninstallOutcome.ReadyToConfirm:
                break;
            default:
                throw new InvalidOperationException($"Outcome tidak dikenal: {plan.Outcome}");
        }

        var app = plan.App!;

        try
        {
            ShortcutUninstallWorkflow.Execute(app, new WindowsProcessRunner());
        }
        catch (UninstallException ex)
        {
            MessageBox.Show(ex.Message, "AppFlush", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            // PENTING: sama seperti di MainForm -- sebelumnya hanya UninstallException
            // yang ditangkap, jadi kegagalan lain (mis. path hasil split tidak valid)
            // bisa lolos tanpa pesan apa pun lewat jalur menu klik-kanan ini juga.
            ShowUnexpectedError(ex);
        }
    }
}
