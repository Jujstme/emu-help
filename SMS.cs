using System;
using System.Runtime.InteropServices;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.SMS;

public class SegaMasterSystem : SMS { }
public class GameGear : SMS { }

public partial class SMS : HelperBase
{
    private SMSBase emu { get; set; }
    private IntPtr ram_base => emu.ram_base;

    public SMS(bool generateCode) : base(generateCode)
    {
        var ProcessNames = new string[]
        {
            "retroarch",
            "blastem",
            "Fusion",
            "mednafen",
        };

        GameProcess = new ProcessHook(ProcessNames);
        Debugs.Info("  => SMS Helper started");
    }

    public SMS()
        : this(true) { }

    protected override void InitActions()
    {
        emu = game.ProcessName.ToLower() switch
        {
            "retroarch" => new Retroarch(this),
            "fusion" => new Fusion(this),
            "blastem" => new BlastEm(this),
            "mednafen" => new Mednafen(this),
            _ => throw new NotImplementedException(),
        };

        KeepAlive = emu.KeepAlive;
        MakeWatchers();
    }

    internal override bool IsAddressInBounds<T>(ulong address)
    {
        var addr = address;

        if (addr >= 0xC000 && addr <= 0xDFFF)
            addr -= 0xC000;
        else if (addr >= 0xE000 && addr <= 0xFFFF)
            addr -= 0xE000;
        else
            return false;
        
        return addr + (ulong)Marshal.SizeOf<T>() <= 0x2000;
    }

    internal override bool IsStringAddressInBounds(ulong address, int stringLength)
    {
        var addr = address;

        if (addr >= 0xC000 && addr <= 0xDFFF)
            addr -= 0xC000;
        else if (addr >= 0xE000 && addr <= 0xFFFF)
            addr -= 0xE000;
        else
            return false;

        return addr + (ulong)stringLength <= 0x2000;
    }

    public override bool TryGetAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;
        
        if (ram_base == null)
            return false;

        var addr = address;
        if (addr >= 0xC000 && addr <= 0xDFFF)
            addr -= 0xC000;
        else if (addr >= 0xE000 && addr <= 0xFFFF)
            addr -= 0xE000;
        else
            return false;

        realAddress = (IntPtr)((ulong)ram_base + addr);
        return true;
    }
}