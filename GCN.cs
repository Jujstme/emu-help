using System;
using System.Runtime.InteropServices;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.GCN;

public class Gamecube : GCN { }

public class GameCube : GCN { }

public partial class GCN : HelperBase
{
    private GCNBase emu;
    internal override Endianness.Endian Endian => emu.Endian;


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
        MakeWatchers();
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

        if (emu == null || emu.MEM1 == null)
            return false;

        ulong defOffset;

        if (address >= 0x80000000 && address < 0x80200000)
            defOffset = address - 0x80000000;
        else
            return false;

        realAddress = (IntPtr)((ulong)emu.MEM1 + defOffset);
        return true;
    }
}