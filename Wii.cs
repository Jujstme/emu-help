using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.WII;

public class Wii : WII { }

public partial class WII : HelperBase
{
    private WIIBase emu;
    private IntPtr MEM1 => emu.MEM1;
    private IntPtr MEM2 => emu.MEM2;
    private Endianness.Endian Endian => emu.Endian;


    public WII(bool generateCode) : base(generateCode)
    {
        var ProcessNames = new string[]
        {
            "Dolphin",
            "retroarch",
        };

        GameProcess = new ProcessHook(ProcessNames);
        Debugs.Info("  => WII Helper started");
    }

    public WII()
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

    public T ReadValue<T>(uint offset, params uint[] offsets) where T : unmanaged
    {
        return ReadValueIgnoringEndianness<T>(offsets.Length == 0 ? offset : DerefOffsets(offset, offsets)).FromEndian(Endian);
    }

    private T ReadValueIgnoringEndianness<T>(uint offset) where T : unmanaged
    {
        if (MEM1 == null || MEM2 == null)
            return default;

        if (offset >= 0x80000000 && offset <= 0x817FFFFF)
            return game.ReadValue<T>((IntPtr)((long)MEM1 + offset - 0x80000000));
        else if (offset >= 0x90000000 && offset <= 0x93FFFFFF)
            return game.ReadValue<T>((IntPtr)((long)MEM2 + offset - 0x90000000));
        else
            return default;
    }

    public string ReadString(int length, uint offset, params uint[] offsets)
    {
        return ReadRawString(length, offsets.Length == 0 ? offset : DerefOffsets(offset, offsets));
    }

    private string ReadRawString(int length, uint offset)
    {
        if (MEM1 == null || MEM2 == null)
            return default;

        if (offset >= 0x80000000 && offset <= 0x817FFFFF)
            return game.ReadString((IntPtr)((long)MEM1 + offset - 0x80000000), length);
        if (offset >= 0x90000000 || offset <= 0x93FFFFFF)
            return game.ReadString((IntPtr)((long)MEM2 + offset - 0x90000000), length);
        else
            return default;
    }

    private uint DerefOffsets(uint offset, uint[] offsets)
    {
        uint actualOffset = ReadValue<uint>(offset);
        for (int i = 0; i < offsets.Length - 1; i++)
            actualOffset = ReadValue<uint>(actualOffset + offsets[i]);
        return actualOffset + offsets[offsets.Length - 1];
    }
}