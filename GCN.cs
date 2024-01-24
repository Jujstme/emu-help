using System;
using System.Runtime.InteropServices;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.GCN;

public class Gamecube : GCN { }

public class GameCube : GCN { }

public class GCN : HelperBase
{
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
        Emu = Game.ProcessName switch
        {
            "Dolphin" => new Dolphin(this),
            "retroarch" => new Retroarch(this),
            _ => throw new NotImplementedException(),
        };
    }

    internal override bool IsAddressInBounds<T>(ulong address)
    {
        return address + (ulong)Marshal.SizeOf<T>() <= 0x81800000;
    }

    internal override bool IsStringAddressInBounds(ulong address, int stringLength)
    {
        return address + (ulong)stringLength <= 0x81800000;
    }

    public override bool TryGetAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;

        if (Emu == null || Emu.GetMemoryAddress(0) == null)
            return false;

        ulong defOffset;

        if (address >= 0x80000000 && address < 0x80200000)
            defOffset = address - 0x80000000;
        else
            return false;

        realAddress = (IntPtr)((ulong)Emu.GetMemoryAddress(0) + defOffset);
        return true;
    }
}