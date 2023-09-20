using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.PS1;

public class Playstation : PS1 { }

public class Playstation1 : PS1 { }

public partial class PS1 : HelperBase
{
    private PS1Base emu;
    private IntPtr emu_base => emu.ram_base;

    public PS1(bool generateCode) : base(generateCode)
    {
        var ProcessNames = new string[]
        {
            "ePSXe",
            "psxfin",
            "duckstation-qt-x64-ReleaseLTCG",
            "duckstation-nogui-x64-ReleaseLTCG",
            "retroarch",
            "pcsx-redux.main",
            "xebra",
            "mednafen"
        };

        GameProcess = new ProcessHook(ProcessNames);
        Debugs.Info("  => PS1 Helper started");
    }

    public PS1()
        : this(true) { }

    protected override void InitActions()
    {
        emu = game.ProcessName.ToLower() switch {
            "epsxe" => new ePSXe(this),
            "duckstation-qt-x64-releaseltcg" or "duckstation-nogui-x64-releaseltcg" => new Duckstation(this),
            "psxfin" => new pSX(this),
            "xebra" => new Xebra(this),
            "retroarch" => new Retroarch(this),
            "pcsx-redux.main" => new PcsxRedux(this),
            "mednafen" => new Mednafen(this),
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
        if (emu_base == null)
            return default;

        var defOffset = offset;

        if (defOffset >= 0x80000000 && defOffset < 0x80200000)
            defOffset -= 0x80000000;
        else if ((defOffset > 0x1FFFFF && defOffset < 0x80000000) || defOffset > 0x801FFFFF)
            return default;

        return game.ReadValue<T>((IntPtr)((long)emu_base + defOffset));
    }
    
    public string ReadString(int length, uint offset)
    {
        if (emu_base == null)
            return default;

        var defOffset = offset;

        if (defOffset >= 0x80000000 && defOffset < 0x80200000)
            defOffset -= 0x80000000;
        else if ((defOffset > 0x1FFFFF && defOffset < 0x80000000) || defOffset > 0x801FFFFF)
            return default;

        return game.ReadString((IntPtr)((long)emu_base + defOffset), length);
    }

    enum Emulator
    {
        ePSXe,
        pSX,
        Duckstation,
        Retroarch,
        PCSXRedux,
        Xebra,
    }
}