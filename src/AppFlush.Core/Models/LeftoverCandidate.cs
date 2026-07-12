namespace AppFlush.Core.Models;

/// <summary>Folder yang dicurigai sebagai sisa aplikasi yang sudah diuninstall.</summary>
public sealed record LeftoverCandidate(string Path, string Reason);
