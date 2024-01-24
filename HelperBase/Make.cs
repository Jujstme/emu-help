using System;
using System.Collections.Generic;
using LiveSplit.EMUHELP;

public abstract partial class HelperBase
{
    private Dictionary<string, Tuple<TypeCode, uint, uint[]>> _load = new();
    private Dictionary<string, int> _stringLoad = new();
    private FakeMemoryWatcherList _watchers = new();

    public FakeMemoryWatcher this[string index] => _watchers[index];
    public Func<HelperBase, bool> Load { get; set; } = null;

    public void Make<T>(string name, uint offset, params uint[] offsets) where T : struct
    {
        _load[name] = Tuple.Create(Type.GetTypeCode(typeof(T)), offset, offsets);
    }

    public void MakeString(string name, int length, uint offset, params uint[] offsets)
    {
        _load[name] = Tuple.Create(TypeCode.String, offset, offsets);
        _stringLoad[name] = length;
    }

    private void MakeWatchers()
    {
        _watchers.Clear();
        foreach (var watcher in _load)
        {
            _watchers.Add(watcher.Value.Item1 switch
            {
                TypeCode.Boolean => new FakeMemoryWatcher<bool>(() => ReadValue<bool>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Char => new FakeMemoryWatcher<char>(() => ReadValue<char>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.SByte => new FakeMemoryWatcher<sbyte>(() => ReadValue<sbyte>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Byte => new FakeMemoryWatcher<byte>(() => ReadValue<byte>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Int16 => new FakeMemoryWatcher<short>(() => ReadValue<short>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.UInt16 => new FakeMemoryWatcher<ushort>(() => ReadValue<ushort>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Int32 => new FakeMemoryWatcher<int>(() => ReadValue<int>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.UInt32 => new FakeMemoryWatcher<uint>(() => ReadValue<uint>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Int64 => new FakeMemoryWatcher<long>(() => ReadValue<long>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.UInt64 => new FakeMemoryWatcher<ulong>(() => ReadValue<ulong>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Single => new FakeMemoryWatcher<float>(() => ReadValue<float>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Double => new FakeMemoryWatcher<double>(() => ReadValue<double>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Decimal => new FakeMemoryWatcher<decimal>(() => ReadValue<decimal>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.DateTime => new FakeMemoryWatcher<DateTime>(() => ReadValue<DateTime>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.String => new FakeMemoryWatcher<string>(() => ReadString(watcher.Value.Item2, _stringLoad[watcher.Key], watcher.Value.Item3)) { Name = watcher.Key },
                _ => throw new NotImplementedException(),
            });
        }
    }
}
