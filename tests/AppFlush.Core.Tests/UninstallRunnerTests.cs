using AppFlush.Core.Models;
using AppFlush.Core.Services;
using AppFlush.Core.Tests.Fakes;
using Xunit;

namespace AppFlush.Core.Tests;

public class UninstallRunnerTests
{
    [Fact]
    public void CallsRunnerWithBuiltCommand()
    {
        var app = new InstalledApp { Name = "Fake App", UninstallString = "uninstall.exe /silent" };
        var runner = new FakeProcessRunner { ExitCode = 0 };

        var exitCode = UninstallRunner.Run(app, runner);

        Assert.Equal(0, exitCode);
        Assert.Equal(new List<string> { "uninstall.exe /silent" }, runner.Commands);
    }

    [Fact]
    public void PropagatesNonZeroExitCode()
    {
        var app = new InstalledApp { Name = "Fake App", UninstallString = "uninstall.exe" };
        var runner = new FakeProcessRunner { ExitCode = 1223 }; // ERROR_CANCELLED (user clicked Cancel)

        var exitCode = UninstallRunner.Run(app, runner);

        Assert.Equal(1223, exitCode);
    }
}
