using AppFlush.Core.Models;
using AppFlush.Core.Services;
using Xunit;

namespace AppFlush.Core.Tests;

public class AppMatcherTests
{
    private static InstalledApp MakeApp(string name, string? installLocation = null) =>
        new() { Name = name, InstallLocation = installLocation };

    [Fact]
    public void FindByQueryIsCaseInsensitive()
    {
        var apps = new[] { MakeApp("Zoom Workplace"), MakeApp("Notion") };

        var matches = AppMatcher.FindByQuery(apps, "zoom");

        Assert.Single(matches);
        Assert.Equal("Zoom Workplace", matches[0].Name);
    }

    [Fact]
    public void FindByShortcutTargetMatchesByInstallLocation()
    {
        var apps = new[]
        {
            MakeApp("Zoom Workplace", @"C:\Users\dedek\AppData\Roaming\Zoom"),
            MakeApp("Notion", @"C:\Users\dedek\AppData\Local\Notion"),
        };

        var matches = AppMatcher.FindByShortcutTarget(apps, @"C:\Users\dedek\AppData\Roaming\Zoom\bin\Zoom.exe");

        Assert.Single(matches);
        Assert.Equal("Zoom Workplace", matches[0].Name);
    }

    [Fact]
    public void FindByShortcutTargetFallsBackToNameMatch()
    {
        var apps = new[] { MakeApp("Zoom Workplace"), MakeApp("Notion") };

        var matches = AppMatcher.FindByShortcutTarget(apps, @"C:\Somewhere\Zoom.exe");

        Assert.Single(matches);
        Assert.Equal("Zoom Workplace", matches[0].Name);
    }

    [Fact]
    public void FindByShortcutTargetReturnsEmptyWhenNoMatch()
    {
        var apps = new[] { MakeApp("Notion") };

        var matches = AppMatcher.FindByShortcutTarget(apps, @"C:\Somewhere\unknown.exe");

        Assert.Empty(matches);
    }
}
