using System;
using System.Runtime.InteropServices;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.WII;

public class Wii : WII { }

public partial class WII : HelperBase
{
    private WIIBase emu;
    private IntPtr MEM1 => emu.MEM1;
    private IntPtr MEM2 => emu.MEM2;
    internal override Endianness.Endian Endian => emu.Endian;


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
        MakeWatchers();
    }

    internal override bool IsAddressInBounds<T>(ulong address)
    {
        if (address >= 0x80000000 && address <= 0x817FFFFF)
            return address + (ulong)Marshal.SizeOf<T>() <= 0x81800000;
        else if (address >= 0x90000000 && address <= 0x93FFFFFF)
            return address + (ulong)Marshal.SizeOf<T>() <= 0x94000000;
        else
            return false;
    }

    internal override bool IsStringAddressInBounds(ulong address, int stringLength)
    {
        if (address >= 0x80000000 && address <= 0x817FFFFF)
            return address + (ulong)stringLength <= 0x81800000;
        else if (address >= 0x90000000 && address <= 0x93FFFFFF)
            return address + (ulong)stringLength <= 0x94000000;
        else
            return false;
    }

    public override bool TryGetAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;

        if (MEM1 == null || MEM2 == null)
            return false;

        if (address >= 0x80000000 && address <= 0x817FFFFF)
            realAddress = (IntPtr)((ulong)MEM1 + address - 0x80000000);
        else if (address >= 0x90000000 && address <= 0x93FFFFFF)
            realAddress = (IntPtr)((ulong)MEM2 + address - 0x90000000);

        return realAddress != default;
    }
}