using AppFlush.Core.Models;

namespace AppFlush.Core.Services;

public static class UninstallRunner
{
    public static int Run(InstalledApp app, IProcessRunner runner)
    {
        var command = UninstallCommandBuilder.Build(app);
        return runner.Run(command);
    }
}
