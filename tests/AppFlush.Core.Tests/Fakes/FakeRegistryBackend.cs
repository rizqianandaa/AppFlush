using AppFlush.Core.RegistryAccess;

namespace AppFlush.Core.Tests.Fakes;

/// <summary>Registry palsu di memori, untuk unit test (setara InMemoryRegistryBackend di versi Python).</summary>
public sealed class FakeRegistryBackend : IRegistryBackend
{
    public List<Dictionary<string, string>> UninstallEntries { get; } = new();
    private readonly Dictionary<(RegistryHive, string), Dictionary<string, string>> _keys = new();

    public IEnumerable<IReadOnlyDictionary<string, string>> EnumerateUninstallEntries() => UninstallEntries;

    public IReadOnlyDictionary<string, string>? ReadValues(RegistryHive hive, string subKeyPath) =>
        _keys.TryGetValue((hive, subKeyPath), out var values) ? values : null;

    public void SetValue(RegistryHive hive, string subKeyPath, string valueName, string value)
    {
        var key = (hive, subKeyPath);
        if (!_keys.TryGetValue(key, out var values))
        {
            values = new Dictionary<string, string>();
            _keys[key] = values;
        }
        values[valueName] = value;
    }

    public void DeleteTree(RegistryHive hive, string subKeyPath)
    {
        var prefix = subKeyPath.TrimEnd('\\');
        var toRemove = _keys.Keys
            .Where(k => k.Item1 == hive && (k.Item2 == prefix || k.Item2.StartsWith(prefix + "\\", StringComparison.OrdinalIgnoreCase)))
            .ToList();
        foreach (var key in toRemove) _keys.Remove(key);
    }
}
