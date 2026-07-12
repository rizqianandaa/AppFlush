namespace AppFlush.Core.Models;

/// <summary>Satu baris aplikasi terpasang, dibaca dari Registry Uninstall key.</summary>
public sealed class InstalledApp
{
    public required string Name { get; init; }
    public string? Version { get; init; }
    public string? Publisher { get; init; }
    public string? UninstallString { get; init; }
    public string? InstallLocation { get; init; }
    public bool IsMsi { get; init; }

    /// <summary>Path lengkap key registry asal entri ini, dipakai untuk debug/telusur.</summary>
    public string RegistryKeyPath { get; init; } = "";

    public override string ToString() => Name;
}
