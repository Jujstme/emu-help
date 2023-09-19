using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.GBA;

public class GameBoyAdvance : GBA { }
public class GameboyAdvance : GBA { }
public class Gameboyadvance : GBA { }

public partial class GBA : HelperBase
{
    private GBABase emu { get; set; }
    private IntPtr ewram => emu.ewram;
    private IntPtr iwram => emu.iwram;

    public GBA(bool generateCode) : base(generateCode)
    {
        var ProcessNames = new string[]
        {
            "visualboyadvance-m",
            "VisualBoyAdvance",
            "mGBA",
            "NO$GBA.EXE",
            "retroarch",
            "EmuHawk",
        };

        GameProcess = new ProcessHook(ProcessNames);
        Debugs.Info("  => GBA Helper started");
    }

    public GBA()
        : this(true) { }

    protected override void InitActions()
    {
        emu = game.ProcessName switch
        {
            "visualboyadvance-m" or "VisualBoyAdvance" => new VisualBoyAdvance(this),
            "mGBA" => new mGBA(this),
            "NO$GBA" => new NoCashGBA(this),
            "retroarch" => new Retroarch(this),
            "EmuHawk" => new EmuHawk(this),
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
        return (offset >> 24) switch {
            2 => ReadFromEWRAM<T>(offset),
            3 => ReadFromIWRAM<T>(offset),
            _ => default,
        };
    }

    private T ReadFromEWRAM<T>(uint offset) where T : struct
    {
        if (ewram == null)
            return default;

        var defOffset = offset;

        if (defOffset >= 0x02000000 && defOffset < 0x02040000)
            defOffset -= 0x02000000;
        else if ((defOffset > 0x3FFFF && defOffset < 0x02000000) || defOffset >= 0x02040000)
            return default;

        return game.ReadValue<T>((IntPtr)((long)ewram + defOffset));
    }

    private T ReadFromIWRAM<T>(uint offset) where T : struct
    {
        if (iwram == null)
            return default;

        var defOffset = offset;

        if (defOffset >= 0x03000000 && defOffset < 0x03008000)
            defOffset -= 0x03000000;
        else if ((defOffset > 0x7FFF && defOffset < 0x03000000) || defOffset >= 0x03008000)
            return default;

        return game.ReadValue<T>((IntPtr)((long)iwram + defOffset));
    }

    public string ReadString(int length, uint offset)
    {
        return (offset >> 24) switch
        {
            2 => ReadStringFromEWRAM(length, offset),
            3 => ReadStringFromIWRAM(length, offset),
            _ => default,
        };
    }

    private string ReadStringFromEWRAM(int length, uint offset)
    {
        if (ewram == null)
            return default;

        var defOffset = offset;

        if (defOffset >= 0x02000000 && defOffset < 0x02040000)
            defOffset -= 0x02000000;
        else if ((defOffset > 0x3FFFF && defOffset < 0x02000000) || defOffset >= 0x02040000)
            return default;

        return game.ReadString((IntPtr)((long)ewram + defOffset), length);
    }

    private string ReadStringFromIWRAM(int length, uint offset)
    {
        if (iwram == null)
            return default;

        var defOffset = offset;

        if (defOffset >= 0x03000000 && defOffset < 0x03008000)
            defOffset -= 0x03000000;
        else if ((defOffset > 0x7FFF && defOffset < 0x03000000) || defOffset >= 0x03008000)
            return default;

        return game.ReadString((IntPtr)((long)iwram + defOffset), length);
    }
}