using AppFlush.Core.Models;
using AppFlush.Core.Services;
using AppFlush.Core.Tests.Fakes;
using AppFlush.Core.Workflows;
using Xunit;

namespace AppFlush.Core.Tests;

public class ShortcutUninstallWorkflowTests
{
    [Fact]
    public void PreparesPlanWhenAppMatches()
    {
        var resolver = new FakeShortcutResolver();
        resolver.TargetsByLnkPath["Zoom.lnk"] = @"C:\Users\dedek\AppData\Roaming\Zoom\bin\Zoom.exe";
        var backend = new FakeRegistryBackend();
        backend.UninstallEntries.Add(new Dictionary<string, string>
        {
            ["DisplayName"] = "Zoom Workplace",
            ["InstallLocation"] = @"C:\Users\dedek\AppData\Roaming\Zoom",
            ["UninstallString"] = @"C:\Users\dedek\AppData\Roaming\Zoom\uninstall\Installer.exe /uninstall",
        });

        var plan = ShortcutUninstallWorkflow.Prepare("Zoom.lnk", resolver, backend);

        Assert.Equal(ShortcutUninstallOutcome.ReadyToConfirm, plan.Outcome);
        Assert.Equal("Zoom Workplace", plan.App!.Name);
        Assert.Contains("Installer.exe", plan.Command);
    }

    [Fact]
    public void ReturnsNoMatchingAppWhenNothingMatches()
    {
        var resolver = new FakeShortcutResolver();
        resolver.TargetsByLnkPath["Unknown.lnk"] = @"C:\Somewhere\unknown.exe";
        var backend = new FakeRegistryBackend();

        var plan = ShortcutUninstallWorkflow.Prepare("Unknown.lnk", resolver, backend);

        Assert.Equal(ShortcutUninstallOutcome.NoMatchingApp, plan.Outcome);
    }

    [Fact]
    public void ReturnsNoTargetPathWhenShortcutHasNoTarget()
    {
        var resolver = new FakeShortcutResolver();
        resolver.TargetsByLnkPath["Broken.lnk"] = null;
        var backend = new FakeRegistryBackend();

        var plan = ShortcutUninstallWorkflow.Prepare("Broken.lnk", resolver, backend);

        Assert.Equal(ShortcutUninstallOutcome.NoTargetPath, plan.Outcome);
    }

    [Fact]
    public void ReturnsShortcutUnreadableWhenResolverThrows()
    {
        var resolver = new FakeShortcutResolver { ExceptionToThrow = new ShortcutParseException("rusak") };
        var backend = new FakeRegistryBackend();

        var plan = ShortcutUninstallWorkflow.Prepare("Bad.lnk", resolver, backend);

        Assert.Equal(ShortcutUninstallOutcome.ShortcutUnreadable, plan.Outcome);
    }

    [Fact]
    public void ExecuteRunsUninstallCommand()
    {
        var app = new InstalledApp { Name = "Fake", UninstallString = "uninstall.exe" };
        var runner = new FakeProcessRunner { ExitCode = 0 };

        var exitCode = ShortcutUninstallWorkflow.Execute(app, runner);

        Assert.Equal(0, exitCode);
        Assert.Single(runner.Commands);
    }
}
