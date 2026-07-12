using AppFlush.Core.Models;
using AppFlush.Core.Services;
using AppFlush.Core.Tests.Fakes;
using Xunit;

namespace AppFlush.Core.Tests;

public class LeftoverScannerTests
{
    [Fact]
    public void FindsFolderMatchingAppName()
    {
        var fs = new FakeFileSystem();
        fs.DirectoriesByRoot["C:\\ProgramData"] = new List<string> { "C:\\ProgramData\\FakeApp" };
        var app = new InstalledApp { Name = "Fake App" };

        var candidates = LeftoverScanner.FindLeftoverDirs(app, fs, new[] { "C:\\ProgramData" });

        Assert.Single(candidates);
        Assert.Equal("C:\\ProgramData\\FakeApp", candidates[0].Path);
    }

    [Fact]
    public void FindsFolderMatchingPublisherWhenNameDoesNotMatch()
    {
        var fs = new FakeFileSystem();
        fs.DirectoriesByRoot["C:\\ProgramData"] = new List<string> { "C:\\ProgramData\\AcmeCorp" };
        var app = new InstalledApp { Name = "Widget", Publisher = "Acme Corp" };

        var candidates = LeftoverScanner.FindLeftoverDirs(app, fs, new[] { "C:\\ProgramData" });

        Assert.Single(candidates);
    }

    [Fact]
    public void ReturnsEmptyWhenNoMatch()
    {
        var fs = new FakeFileSystem();
        fs.DirectoriesByRoot["C:\\ProgramData"] = new List<string> { "C:\\ProgramData\\Unrelated" };
        var app = new InstalledApp { Name = "Fake App" };

        var candidates = LeftoverScanner.FindLeftoverDirs(app, fs, new[] { "C:\\ProgramData" });

        Assert.Empty(candidates);
    }

    [Fact]
    public void DeleteCandidatesOnlyDeletesExistingDirs()
    {
        var fs = new FakeFileSystem();
        fs.DirectoriesByRoot["C:\\ProgramData"] = new List<string> { "C:\\ProgramData\\FakeApp" };

        var deleted = LeftoverScanner.DeleteCandidates(fs, new[] { "C:\\ProgramData\\FakeApp", "C:\\Missing" });

        Assert.Equal(new List<string> { "C:\\ProgramData\\FakeApp" }, deleted);
        Assert.Equal(new List<string> { "C:\\ProgramData\\FakeApp" }, fs.Deleted);
    }
}
