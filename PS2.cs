using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.PS2;

public class Playstation2 : PS2 { }

public partial class PS2 : HelperBase
{
    private PS2Base emu;
    private IntPtr emu_base => emu.ram_base;

    public PS2(bool generateCode) : base(generateCode)
    {
        var ProcessNames = new string[]
        {
            "pcsx2x64",
            "pcsx2-qt",
            "pcsx2x64-avx2",
            "pcsx2-avx2",
            "pcsx2",
            "retroarch",
        };

        GameProcess = new ProcessHook(ProcessNames);
        Debugs.Info("  => PS2 Helper started");
    }

    public PS2()
        : this(true) { }

    protected override void InitActions()
    {
        emu = game.ProcessName.ToLower() switch {
            "pcsx2x64" or "pcsx2-qt" or "pcsx2x64-avx2" or "pcsx2-avx2" or "pcsx2" => new PCSX2(this),
            "retroarch" => new Retroarch(this),
            _ => throw new NotImplementedException(),
        };

        KeepAlive = emu.KeepAlive;

        Watchers = new();
        foreach (var watcher in _load)
        {
            FakeMemoryWatcher newWatcher = watcher.Value.Item1 switch
            {
                TypeCode.Int32 => new FakeMemoryWatcher<int>(() => ReadValue<int>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Boolean => new FakeMemoryWatcher<bool>(() => ReadValue<bool>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Char => new FakeMemoryWatcher<char>(() => ReadValue<char>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.SByte => new FakeMemoryWatcher<sbyte>(() => ReadValue<sbyte>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Byte => new FakeMemoryWatcher<byte>(() => ReadValue<byte>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Int16 => new FakeMemoryWatcher<short>(() => ReadValue<short>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.UInt16 => new FakeMemoryWatcher<ushort>(() => ReadValue<ushort>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.UInt32 => new FakeMemoryWatcher<uint>(() => ReadValue<uint>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Int64 => new FakeMemoryWatcher<long>(() => ReadValue<long>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.UInt64 => new FakeMemoryWatcher<ulong>(() => ReadValue<ulong>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Single => new FakeMemoryWatcher<float>(() => ReadValue<float>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Double => new FakeMemoryWatcher<double>(() => ReadValue<double>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.Decimal => new FakeMemoryWatcher<decimal>(() => ReadValue<decimal>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.DateTime => new FakeMemoryWatcher<DateTime>(() => ReadValue<DateTime>(watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                TypeCode.String => new FakeMemoryWatcher<string>(() => ReadString(_stringLoad[watcher.Key], watcher.Value.Item2, watcher.Value.Item3)) { Name = watcher.Key },
                _ => throw new NotImplementedException(),
            };
            Watchers.Add(newWatcher);
        }
    }

    public T ReadValue<T>(uint offset, params uint[] offsets) where T : struct
    {
        if (emu_base == null)
            return default;

        if (offset < 0x00100000 || offset > 0x01FFFFFF)
            return default;

        return game.ReadValue<T>((IntPtr)((long)emu_base + (offsets.Length == 0 ? offset : DerefOffsets(offset, offsets))));
    }
    
    public string ReadString(int length, uint offset, params uint[] offsets)
    {
        if (emu_base == null)
            return default;

        if (offset < 0x00100000 || offset > 0x01FFFFFF)
            return default;

        return game.ReadString((IntPtr)((long)emu_base + (offsets.Length == 0 ? offset : DerefOffsets(offset, offsets))), length);
    }

    private uint DerefOffsets(uint offset, uint[] offsets)
    {
        uint actualOffset = ReadValue<uint>(offset);
        for (int i = 0; i < offsets.Length - 1; i++)
            actualOffset = ReadValue<uint>(actualOffset + offsets[i]);
        return actualOffset + offsets[offsets.Length - 1];
    }
}