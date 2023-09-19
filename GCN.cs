using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.GCN;

public class Gamecube : GCN { }

public class GameCube : GCN { }

public partial class GCN : HelperBase
{
    private GCNBase emu;
    private IntPtr MEM1 => emu.MEM1;
    private Endianness.Endian Endian => emu.Endian;


    public GCN(bool generateCode) : base(generateCode)
    {
        var ProcessNames = new string[]
        {
            "Dolphin",
            "retroarch",
        };

        GameProcess = new ProcessHook(ProcessNames);
        Debugs.Info("  => GCN Helper started");
    }

    public GCN()
        : this(true) { }

    protected override void InitActions()
    {
        emu = game.ProcessName switch
        {
            "Dolphin" => new Dolphin(this),
            "retroarch" => new Retroarch(this),
            _ => throw new NotImplementedException(),
        };

        KeepAlive = emu.KeepAlive;

        Watchers = new();
        foreach (var watcher in _load)
        {
            FakeMemoryWatcher newWatcher = watcher.Value.Item1 switch
            {
                TypeCode.Int32 => new FakeMemoryWatcher<int>(() => ReadValue<int>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.Boolean => new FakeMemoryWatcher<bool>(() => ReadValue<bool>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.Char => new FakeMemoryWatcher<char>(() => ReadValue<char>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.SByte => new FakeMemoryWatcher<sbyte>(() => ReadValue<sbyte>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.Byte => new FakeMemoryWatcher<byte>(() => ReadValue<byte>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.Int16 => new FakeMemoryWatcher<short>(() => ReadValue<short>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.UInt16 => new FakeMemoryWatcher<ushort>(() => ReadValue<ushort>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.UInt32 => new FakeMemoryWatcher<uint>(() => ReadValue<uint>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.Int64 => new FakeMemoryWatcher<long>(() => ReadValue<long>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.UInt64 => new FakeMemoryWatcher<ulong>(() => ReadValue<ulong>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.Single => new FakeMemoryWatcher<float>(() => ReadValue<float>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.Double => new FakeMemoryWatcher<double>(() => ReadValue<double>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.Decimal => new FakeMemoryWatcher<decimal>(() => ReadValue<decimal>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.DateTime => new FakeMemoryWatcher<DateTime>(() => ReadValue<DateTime>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.String => new FakeMemoryWatcher<string>(() => ReadString(_stringLoad[watcher.Key], watcher.Value.Item2)) { Name = watcher.Key },
                _ => throw new NotImplementedException(),
            };
            Watchers.Add(newWatcher);
        }
    }

    public T ReadValue<T>(uint offset) where T : unmanaged
    {
        if (MEM1 == null)
            return default;

        var defOffset = offset;

        if ((defOffset > 0x017FFFFF && defOffset < 0x80000000) || defOffset > 0x817FFFFF)
            return default;
        else if (defOffset >= 0x80000000 && defOffset <= 0x817FFFFF)
            defOffset -= 0x80000000;

        return game.ReadValue<T>((IntPtr)((long)MEM1 + defOffset)).FromEndian(Endian);
    }
    
    public string ReadString(int length, uint offset)
    {
        if (MEM1 == null)
            return default;

        var defOffset = offset;

        if ((defOffset > 0x017FFFFF && defOffset < 0x80000000) || defOffset > 0x817FFFFF)
            return default;
        else if (defOffset >= 0x80000000 && defOffset <= 0x817FFFFF)
            defOffset -= 0x80000000;

        return game.ReadString((IntPtr)((long)MEM1 + defOffset), length);
    }
}