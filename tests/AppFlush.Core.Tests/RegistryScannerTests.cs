using AppFlush.Core.Services;
using AppFlush.Core.Tests.Fakes;
using Xunit;

namespace AppFlush.Core.Tests;

public class RegistryScannerTests
{
    [Fact]
    public void ScansSingleApp()
    {
        var backend = new FakeRegistryBackend();
        backend.UninstallEntries.Add(new Dictionary<string, string>
        {
            ["DisplayName"] = "Fake App",
            ["DisplayVersion"] = "1.2.3",
            ["Publisher"] = "Fake Publisher",
            ["UninstallString"] = "C:\\Program Files\\FakeApp\\uninstall.exe",
        });

        var apps = RegistryScanner.ScanInstalledApps(backend);

        Assert.Single(apps);
        Assert.Equal("Fake App", apps[0].Name);
        Assert.Equal("1.2.3", apps[0].Version);
    }

    [Fact]
    public void SkipsEntriesWithoutDisplayName()
    {
        var backend = new FakeRegistryBackend();
        backend.UninstallEntries.Add(new Dictionary<string, string> { ["UninstallString"] = "x.exe" });

        var apps = RegistryScanner.ScanInstalledApps(backend);

        Assert.Empty(apps);
    }

    [Fact]
    public void SkipsSystemComponentsByDefault()
    {
        var backend = new FakeRegistryBackend();
        backend.UninstallEntries.Add(new Dictionary<string, string>
        {
            ["DisplayName"] = "Hidden Runtime",
            ["SystemComponent"] = "1",
        });

        var apps = RegistryScanner.ScanInstalledApps(backend);

        Assert.Empty(apps);
    }

    [Fact]
    public void IncludesSystemComponentsWhenRequested()
    {
        var backend = new FakeRegistryBackend();
        backend.UninstallEntries.Add(new Dictionary<string, string>
        {
            ["DisplayName"] = "Hidden Runtime",
            ["SystemComponent"] = "1",
        });

        var apps = RegistryScanner.ScanInstalledApps(backend, includeSystemComponents: true);

        Assert.Single(apps);
    }

    [Fact]
    public void MsiFlagDetected()
    {
        var backend = new FakeRegistryBackend();
        backend.UninstallEntries.Add(new Dictionary<string, string>
        {
            ["DisplayName"] = "Msi App",
            ["WindowsInstaller"] = "1",
            ["UninstallString"] = "MsiExec.exe /I{GUID}",
        });

        var apps = RegistryScanner.ScanInstalledApps(backend);

        Assert.True(apps[0].IsMsi);
    }

    [Fact]
    public void ResultsAreSortedByName()
    {
        var backend = new FakeRegistryBackend();
        backend.UninstallEntries.Add(new Dictionary<string, string> { ["DisplayName"] = "Zebra App" });
        backend.UninstallEntries.Add(new Dictionary<string, string> { ["DisplayName"] = "Alpha App" });

        var apps = RegistryScanner.ScanInstalledApps(backend);

        Assert.Equal("Alpha App", apps[0].Name);
        Assert.Equal("Zebra App", apps[1].Name);
    }
}
