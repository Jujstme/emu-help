using System;
using System.Runtime.InteropServices;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.GBC;

public class GameBoyColor : GBC { }
public class GameboyColor : GBC { }
public class Gameboycolor : GBC { }

public class GBC : HelperBase
{
    public GBC(bool generateCode) : base(generateCode)
    {
        var ProcessNames = new string[]
        {
            "GSE",
            "gambatte_speedrun"
        };

        GameProcess = new ProcessHook(ProcessNames);
        Debugs.Info("  => GBC Helper started");
    }

    public GBC()
        : this(true) { }

    protected override void InitActions()
    {
        Emu = Game.ProcessName switch
        {
            "GSE" => new GSE(this),
            "gambatte_speedrun" => new GSR_qt(this),
            _ => throw new NotImplementedException(),
        };
    }

    internal override bool IsAddressInBounds<T>(ulong address)
    {
        return address + (ulong)Marshal.SizeOf<T>() <= 0x10000;
    }

    internal override bool IsStringAddressInBounds(ulong address, int stringLength)
    {
        return address + (ulong)stringLength <= 0x10000;
    }

    public override bool TryGetAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;

        if (Emu == null || Emu.GetMemoryAddress(0) == null)
            return false;

        int region = 0;
        if (address >= 0xFF00 && address <= 0xFFFF)
        {
            address = address - 0xFF00;
            region = 1;
        }

        realAddress = (IntPtr)((ulong)Emu.GetMemoryAddress(region) + address);
        return true;
    }
}
