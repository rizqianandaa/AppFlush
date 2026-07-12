using AppFlush.Core.Models;
using AppFlush.Core.RegistryAccess;
using AppFlush.Core.Services;

namespace AppFlush.Core.Workflows;

public enum ShortcutUninstallOutcome
{
    ShortcutUnreadable,
    NoTargetPath,
    NoMatchingApp,
    CommandUnavailable,
    ReadyToConfirm,
}

public sealed record ShortcutUninstallPlan(
    ShortcutUninstallOutcome Outcome,
    string? Message,
    InstalledApp? App,
    string? Command,
    IReadOnlyList<InstalledApp> OtherCandidates);

/// <summary>
/// Logika murni (tanpa GUI/console) untuk alur "uninstall dari shortcut .lnk",
/// dipakai baik oleh menu klik-kanan maupun tombol di MainForm. Dipisah dari UI
/// supaya seluruh alurnya bisa diuji lewat xunit tanpa membuka jendela apa pun.
/// </summary>
public static class ShortcutUninstallWorkflow
{
    public static ShortcutUninstallPlan Prepare(
        string lnkPath, IShortcutResolver resolver, IRegistryBackend backend)
    {
        string? targetPath;
        try
        {
            targetPath = resolver.ResolveTargetPath(lnkPath);
        }
        catch (ShortcutParseException ex)
        {
            return new ShortcutUninstallPlan(
                ShortcutUninstallOutcome.ShortcutUnreadable, ex.Message, null, null, Array.Empty<InstalledApp>());
        }

        if (string.IsNullOrWhiteSpace(targetPath))
        {
            return new ShortcutUninstallPlan(
                ShortcutUninstallOutcome.NoTargetPath,
                "Tidak bisa menemukan target dari shortcut ini.",
                null, null, Array.Empty<InstalledApp>());
        }

        var apps = RegistryScanner.ScanInstalledApps(backend);
        var matches = AppMatcher.FindByShortcutTarget(apps, targetPath);
        if (matches.Count == 0)
        {
            return new ShortcutUninstallPlan(
                ShortcutUninstallOutcome.NoMatchingApp,
                "Tidak menemukan aplikasi terpasang yang cocok dengan shortcut ini.",
                null, null, Array.Empty<InstalledApp>());
        }

        var app = matches[0];
        var others = matches.Skip(1).ToList();

        string command;
        try
        {
            command = UninstallCommandBuilder.Build(app);
        }
        catch (UninstallException ex)
        {
            return new ShortcutUninstallPlan(
                ShortcutUninstallOutcome.CommandUnavailable, ex.Message, app, null, others);
        }

        return new ShortcutUninstallPlan(
            ShortcutUninstallOutcome.ReadyToConfirm, null, app, command, others);
    }

    public static int Execute(InstalledApp app, IProcessRunner runner) => UninstallRunner.Run(app, runner);
}
