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
            "GSR",
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
            "GSR" => new GSR(this),
            "gambatte_speedrun" => new GSR_qt(this),
            _ => throw new NotImplementedException(),
        };
    }

    internal override bool IsAddressInBounds<T>(ulong address)
    {
        return address + (ulong)Marshal.SizeOf<T>() <= 0xffff;
    }

    internal override bool IsStringAddressInBounds(ulong address, int stringLength)
    {
        return address + (ulong)stringLength <= 0xffff;
    }

    public override bool TryGetAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;

        if (Emu == null || Emu.GetMemoryAddress(0) == null)
            return false;

        int region = 0;
        if (address >= 0xFF00 && address <= 0xFFFE)
        {
            address = address - 0xff00;
            region = 1;
        }

        realAddress = (IntPtr)((ulong)Emu.GetMemoryAddress(region) + address);
        return true;
    }
}
