using System;
using System.Runtime.InteropServices;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.Genesis;

public class Megadrive : Genesis { }

public partial class Genesis : HelperBase
{
    private GenesisBase emu { get; set; }
    private IntPtr ram_base => emu.ram_base;
    private Endianness.Endian Endian => emu.Endian;

    public Genesis(bool generateCode) : base(generateCode)
    {
        var ProcessNames = new string[]
        {
            "retroarch",
            "SEGAGameRoom",
            "SEGAGenesisClassics",
            "Fusion",
            "gens",
            "blastem",
        };

        GameProcess = new ProcessHook(ProcessNames);
        Debugs.Info("  => Genesis Helper started");
    }

    public Genesis()
        : this(true) { }

    protected override void InitActions()
    {
        emu = game.ProcessName switch
        {
            "retroarch" => new Retroarch(this),
            "SEGAGameRoom" or "SEGAGenesisClassics" => new SegaClassics(this),
            "Fusion" => new Fusion(this),
            "gens" => new Gens(this),
            "blastem" => new BlastEm(this),
            _ => throw new NotImplementedException(),
        };

        KeepAlive = emu.KeepAlive;

        Watchers = new();
        foreach (var watcher in _load)
        {
            FakeMemoryWatcher newWatcher = watcher.Value.Item1 switch
            {
                //TypeCode.Int32 => new FakeMemoryWatcher<int>(() => ReadValue<int>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.Boolean => new FakeMemoryWatcher<bool>(() => ReadValue<bool>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.Char => new FakeMemoryWatcher<char>(() => ReadValue<char>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.SByte => new FakeMemoryWatcher<sbyte>(() => ReadValue<sbyte>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.Byte => new FakeMemoryWatcher<byte>(() => ReadValue<byte>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.Int16 => new FakeMemoryWatcher<short>(() => ReadValue<short>(watcher.Value.Item2)) { Name = watcher.Key },
                TypeCode.UInt16 => new FakeMemoryWatcher<ushort>(() => ReadValue<ushort>(watcher.Value.Item2)) { Name = watcher.Key },
                //TypeCode.UInt32 => new FakeMemoryWatcher<uint>(() => ReadValue<uint>(watcher.Value.Item2)) { Name = watcher.Key },
                //TypeCode.Int64 => new FakeMemoryWatcher<long>(() => ReadValue<long>(watcher.Value.Item2)) { Name = watcher.Key },
                //TypeCode.UInt64 => new FakeMemoryWatcher<ulong>(() => ReadValue<ulong>(watcher.Value.Item2)) { Name = watcher.Key },
                //TypeCode.Single => new FakeMemoryWatcher<float>(() => ReadValue<float>(watcher.Value.Item2)) { Name = watcher.Key },
                //TypeCode.Double => new FakeMemoryWatcher<double>(() => ReadValue<double>(watcher.Value.Item2)) { Name = watcher.Key },
                //TypeCode.Decimal => new FakeMemoryWatcher<decimal>(() => ReadValue<decimal>(watcher.Value.Item2)) { Name = watcher.Key },
                //TypeCode.DateTime => new FakeMemoryWatcher<DateTime>(() => ReadValue<DateTime>(watcher.Value.Item2)) { Name = watcher.Key },
                //TypeCode.String => new FakeMemoryWatcher<string>(() => ReadString(_stringLoad[watcher.Key], watcher.Value.Item2)) { Name = watcher.Key },
                _ => throw new NotImplementedException(),
            };
            Watchers.Add(newWatcher);
            Debugs.Info($"Watcher \"{watcher.Key}\" added successfully.");
        }
    }

    public T ReadValue<T>(uint offset) where T : unmanaged
    {
        if (ram_base == null)
            return default;

        var defOffset = offset;

        if ((offset > 0xFFFF && offset < 0xFF0000) || offset > 0xFFFFFF)
            return default;
        else if (offset >= 0xFF0000 && offset <= 0xFFFFFF)
            defOffset -= 0xFF0000;

        uint toggle = Endian == Endianness.Endian.Little && Marshal.SizeOf(typeof(T)) == 1 ? (uint)1 : 0;

        defOffset ^= toggle;

        return game.ReadValue<T>((IntPtr)((long)ram_base + defOffset)).FromEndian(Endian);
    }
}