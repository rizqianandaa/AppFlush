using AppFlush.Core.Services;

namespace AppFlush.Core.Tests.Fakes;

public sealed class FakeShortcutResolver : IShortcutResolver
{
    public Dictionary<string, string?> TargetsByLnkPath { get; } = new();
    public Exception? ExceptionToThrow { get; set; }

    public string? ResolveTargetPath(string lnkPath)
    {
        if (ExceptionToThrow is not null) throw ExceptionToThrow;
        return TargetsByLnkPath.TryGetValue(lnkPath, out var target) ? target : null;
    }
}
