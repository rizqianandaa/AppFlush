using AppFlush.Core.Services;
using AppFlush.Core.Tests.Fakes;
using Xunit;

namespace AppFlush.Core.Tests;

public class ContextMenuRegistrarTests
{
    [Fact]
    public void BuildCommandWrapsExePath()
    {
        var command = ContextMenuRegistrar.BuildCommand("C:\\Program Files\\AppFlush\\AppFlush.exe");

        Assert.Equal("\"C:\\Program Files\\AppFlush\\AppFlush.exe\" from-shortcut \"%1\"", command);
    }

    [Fact]
    public void RegisterThenIsRegisteredReturnsTrue()
    {
        var backend = new FakeRegistryBackend();
        var command = ContextMenuRegistrar.BuildCommand("C:\\App\\AppFlush.exe");

        ContextMenuRegistrar.Register(backend, command);

        Assert.True(ContextMenuRegistrar.IsRegistered(backend));
    }

    [Fact]
    public void UnregisterRemovesMenu()
    {
        var backend = new FakeRegistryBackend();
        ContextMenuRegistrar.Register(backend, ContextMenuRegistrar.BuildCommand("C:\\App\\AppFlush.exe"));

        ContextMenuRegistrar.Unregister(backend);

        Assert.False(ContextMenuRegistrar.IsRegistered(backend));
    }

    [Fact]
    public void IsRegisteredReturnsFalseWhenNeverRegistered()
    {
        var backend = new FakeRegistryBackend();

        Assert.False(ContextMenuRegistrar.IsRegistered(backend));
    }
}
