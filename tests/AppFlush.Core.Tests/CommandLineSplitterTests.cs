using AppFlush.Core.Services;
using Xunit;

namespace AppFlush.Core.Tests;

public class CommandLineSplitterTests
{
    [Fact]
    public void SplitsQuotedPathWithArguments()
    {
        var (fileName, arguments) = CommandLineSplitter.Split(
            "\"C:\\Users\\dedek\\AppData\\Roaming\\Zoom\\uninstall\\Installer.exe\" /uninstall");

        Assert.Equal(@"C:\Users\dedek\AppData\Roaming\Zoom\uninstall\Installer.exe", fileName);
        Assert.Equal("/uninstall", arguments);
    }

    [Fact]
    public void SplitsQuotedPathWithoutArguments()
    {
        var (fileName, arguments) = CommandLineSplitter.Split("\"C:\\Program Files\\App\\uninstall.exe\"");

        Assert.Equal(@"C:\Program Files\App\uninstall.exe", fileName);
        Assert.Equal(string.Empty, arguments);
    }

    [Fact]
    public void SplitsUnquotedPathWithArguments()
    {
        var (fileName, arguments) = CommandLineSplitter.Split("MsiExec.exe /X{GUID}");

        Assert.Equal("MsiExec.exe", fileName);
        Assert.Equal("/X{GUID}", arguments);
    }

    [Fact]
    public void SplitsBareExecutableWithNoArguments()
    {
        var (fileName, arguments) = CommandLineSplitter.Split("uninstall.exe");

        Assert.Equal("uninstall.exe", fileName);
        Assert.Equal(string.Empty, arguments);
    }

    [Fact]
    public void TrimsSurroundingWhitespace()
    {
        var (fileName, arguments) = CommandLineSplitter.Split("   uninstall.exe   /silent   ");

        Assert.Equal("uninstall.exe", fileName);
        Assert.Equal("/silent", arguments);
    }

    // --- Kasus nyata yang menyebabkan bug: path TANPA kutip yang mengandung spasi. ---
    // Contoh asli dari registry aplikasi "Everything": UninstallString =
    // "C:\Program Files (x86)\Everything\Uninstall.exe" (tanpa kutip, tanpa argumen).

    [Fact]
    public void SplitsUnquotedPathWithSpacesAndNoArguments_WhenFullPathExists()
    {
        const string path = @"C:\Program Files (x86)\Everything\Uninstall.exe";

        var (fileName, arguments) = CommandLineSplitter.Split(path, fileExists: candidate => candidate == path);

        Assert.Equal(path, fileName);
        Assert.Equal(string.Empty, arguments);
    }

    [Fact]
    public void SplitsUnquotedPathWithSpacesAndArguments_WhenPrefixMatchesRealFile()
    {
        const string path = @"C:\Program Files (x86)\App\Uninstall.exe";
        var commandLine = path + " /S";

        var (fileName, arguments) = CommandLineSplitter.Split(commandLine, fileExists: candidate => candidate == path);

        Assert.Equal(path, fileName);
        Assert.Equal("/S", arguments);
    }

    [Fact]
    public void FallsBackToFirstSpaceWhenNoPrefixMatchesAnyRealFile()
    {
        var (fileName, arguments) = CommandLineSplitter.Split("MsiExec.exe /X{GUID}", fileExists: _ => false);

        Assert.Equal("MsiExec.exe", fileName);
        Assert.Equal("/X{GUID}", arguments);
    }
}
