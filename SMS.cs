using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.SMS;

public class SegaMasterSystem : SMS { }
public class GameGear : SMS { }

public partial class SMS : HelperBase
{
    private SMSBase emu { get; set; }
    private IntPtr ram_base => emu.ram_base;

    public SMS(bool generateCode) : base(generateCode)
    {
        var ProcessNames = new string[]
        {
            "retroarch",
            "blastem",
            "Fusion",
        };

        GameProcess = new ProcessHook(ProcessNames);
        Debugs.Info("  => SMS Helper started");
    }

    public SMS()
        : this(true) { }

    protected override void InitActions()
    {
        emu = game.ProcessName.ToLower() switch
        {
            "retroarch" => new Retroarch(this),
            "fusion" => new Fusion(this),
            "blastem" => new BlastEm(this),
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

    public T ReadValue<T>(uint offset) where T : struct
    {
        if (ram_base == null)
            return default;

        var defOffset = offset;

        if (defOffset >= 0xC000 && defOffset <= 0xDFFF)
            defOffset -= 0xC000;
        else if (defOffset >= 0xE000 && defOffset <= 0xFFFF)
            defOffset -= 0xE000;
        else if ((defOffset > 0x1FFF && defOffset < 0xC000) || defOffset > 0xFFFF)
            return default;

        return game.ReadValue<T>((IntPtr)((long)ram_base + defOffset));
    }

    public string ReadString(int length, uint offset)
    {
        if (ram_base == null)
            return default;

        var defOffset = offset;

        if (defOffset >= 0xC000 && defOffset <= 0xDFFF)
            defOffset -= 0xC000;
        else if (defOffset >= 0xE000 && defOffset <= 0xFFFF)
            defOffset -= 0xE000;
        else if ((defOffset > 0x1FFF && defOffset < 0xC000) || defOffset > 0xFFFF)
            return default;

        return game.ReadString((IntPtr)((long)ram_base + defOffset), length);
    }
}