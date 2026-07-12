using System.Text.RegularExpressions;
using AppFlush.Core.Models;

namespace AppFlush.Core.Services;

public sealed class UninstallException : Exception
{
    public UninstallException(string message) : base(message) { }
}

/// <summary>Menyusun perintah uninstall final dari UninstallString di registry.</summary>
public static class UninstallCommandBuilder
{
    public static string Build(InstalledApp app)
    {
        if (string.IsNullOrWhiteSpace(app.UninstallString))
            throw new UninstallException($"Aplikasi '{app.Name}' tidak punya perintah uninstall yang terdaftar di registry.");

        var command = app.UninstallString.Trim();

        // Installer MSI biasanya mendaftarkan UninstallString dengan "/I{GUID}"
        // (padahal itu perintah *install/repair*) -- ganti jadi "/X{GUID}" (uninstall).
        if (app.IsMsi && Regex.IsMatch(command, @"/I\{", RegexOptions.IgnoreCase))
        {
            command = Regex.Replace(command, @"/I\{", "/X{", RegexOptions.IgnoreCase);
        }

        return command;
    }
}
