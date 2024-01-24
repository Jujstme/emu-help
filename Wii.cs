using System;
using System.Runtime.InteropServices;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.WII;

public class Wii : WII { }

public class WII : HelperBase
{
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
        Emu = Game.ProcessName switch
        {
            "Dolphin" => new Dolphin(this),
            "retroarch" => new Retroarch(this),
            _ => throw new NotImplementedException(),
        };
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

        if (Emu.GetMemoryAddress(0) == null || Emu.GetMemoryAddress(1) == null)
            return false;

        if (address >= 0x80000000 && address <= 0x817FFFFF)
            realAddress = (IntPtr)((ulong)Emu.GetMemoryAddress(0) + address - 0x80000000);
        else if (address >= 0x90000000 && address <= 0x93FFFFFF)
            realAddress = (IntPtr)((ulong)Emu.GetMemoryAddress(1) + address - 0x90000000);

        return realAddress != default;
    }
}