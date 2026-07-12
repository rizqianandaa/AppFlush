using AppFlush.Core.Models;
using AppFlush.Core.Services;
using Xunit;

namespace AppFlush.Core.Tests;

public class UninstallCommandBuilderTests
{
    [Fact]
    public void BuildsSimpleCommand()
    {
        var app = new InstalledApp { Name = "Fake App", UninstallString = "C:\\Program Files\\FakeApp\\uninstall.exe" };

        var command = UninstallCommandBuilder.Build(app);

        Assert.Equal("C:\\Program Files\\FakeApp\\uninstall.exe", command);
    }

    [Fact]
    public void ConvertsMsiInstallFlagToUninstallFlag()
    {
        var app = new InstalledApp
        {
            Name = "Msi App",
            IsMsi = true,
            UninstallString = "MsiExec.exe /I{11111111-2222-3333-4444-555555555555}",
        };

        var command = UninstallCommandBuilder.Build(app);

        Assert.Contains("/X{", command);
        Assert.DoesNotContain("/I{", command);
    }

    [Fact]
    public void ThrowsWhenUninstallStringMissing()
    {
        var app = new InstalledApp { Name = "No Uninstaller" };

        Assert.Throws<UninstallException>(() => UninstallCommandBuilder.Build(app));
    }

    [Fact]
    public void ThrowsWhenUninstallStringBlank()
    {
        var app = new InstalledApp { Name = "Blank", UninstallString = "   " };

        Assert.Throws<UninstallException>(() => UninstallCommandBuilder.Build(app));
    }
}
