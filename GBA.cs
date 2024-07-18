using System;
using System.Runtime.InteropServices;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.GBA;

public class GameBoyAdvance : GBA { }
public class GameboyAdvance : GBA { }
public class Gameboyadvance : GBA { }

public class GBA : HelperBase
{
    public GBA(bool generateCode) : base(generateCode)
    {
        var ProcessNames = new string[]
        {
            "visualboyadvance-m",
            "VisualBoyAdvance",
            "mGBA",
            "NO$GBA.EXE",
            "retroarch",
            "EmuHawk",
            "mednafen",
            "GSE",
        };

        GameProcess = new ProcessHook(ProcessNames);
        Debugs.Info("  => GBA Helper started");
    }

    public GBA()
        : this(true) { }

    protected override void InitActions()
    {
        Emu = Game.ProcessName switch
        {
            "visualboyadvance-m" or "VisualBoyAdvance" => new VisualBoyAdvance(this),
            "mGBA" => new mGBA(this),
            "NO$GBA" => new NoCashGBA(this),
            "retroarch" => new Retroarch(this),
            "EmuHawk" => new EmuHawk(this),
            "mednafen" => new Mednafen(this),
            "GSE" => new GSE(this),
            _ => throw new NotImplementedException(),
        };
    }

    internal override bool IsAddressInBounds<T>(ulong address)
    {
        return (address >> 24) switch
        {
            2 => address + (ulong)Marshal.SizeOf<T>() <= 0x02040000,
            3 => address + (ulong)Marshal.SizeOf<T>() <= 0x03008000,
            _ => false,
        };
    }

    internal override bool IsStringAddressInBounds(ulong address, int stringLength)
    {
        return (address >> 24) switch
        {
            2 => address + (ulong)stringLength <= 0x02040000,
            3 => address + (ulong)stringLength <= 0x03008000,
            _ => false,
        };
    }

    public override bool TryGetAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = (address >> 24) switch
        {
            2 => Emu.GetMemoryAddress(0) != null && address < 0x02040000 ? (IntPtr)((ulong)Emu.GetMemoryAddress(0) + (address - 0x02000000)) : default,
            3 => Emu.GetMemoryAddress(1) != null && address < 0x03008000 ? (IntPtr)((ulong)Emu.GetMemoryAddress(1) + (address - 0x03000000)) : default,
            _ => default,
        };

        return realAddress != default;
    }
}