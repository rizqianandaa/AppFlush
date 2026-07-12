using AppFlush.Core.Services;

namespace AppFlush.Core.Tests.Fakes;

public sealed class FakeProcessRunner : IProcessRunner
{
    public List<string> Commands { get; } = new();
    public int ExitCode { get; set; } = 0;

    public int Run(string commandLine)
    {
        Commands.Add(commandLine);
        return ExitCode;
    }
}
