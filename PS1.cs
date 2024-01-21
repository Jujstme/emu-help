using System;
using System.Runtime.InteropServices;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.PS1;

public class Playstation : PS1 { }

public class Playstation1 : PS1 { }

public partial class PS1 : HelperBase
{
    private PS1Base emu;
    private IntPtr emu_base => emu.ram_base;

    public PS1(bool generateCode) : base(generateCode)
    {
        var ProcessNames = new string[]
        {
            "ePSXe",
            "psxfin",
            "duckstation-qt-x64-ReleaseLTCG",
            "duckstation-nogui-x64-ReleaseLTCG",
            "retroarch",
            "pcsx-redux.main",
            "xebra",
            "mednafen"
        };

        GameProcess = new ProcessHook(ProcessNames);
        Debugs.Info("  => PS1 Helper started");
    }

    public PS1()
        : this(true) { }

    protected override void InitActions()
    {
        emu = game.ProcessName.ToLower() switch {
            "epsxe" => new ePSXe(this),
            "duckstation-qt-x64-releaseltcg" or "duckstation-nogui-x64-releaseltcg" => new Duckstation(this),
            "psxfin" => new pSX(this),
            "xebra" => new Xebra(this),
            "retroarch" => new Retroarch(this),
            "pcsx-redux.main" => new PcsxRedux(this),
            "mednafen" => new Mednafen(this),
            _ => throw new NotImplementedException(),
        };

        KeepAlive = emu.KeepAlive;
        MakeWatchers();
    }

    public override bool TryGetAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;

        if (emu_base == null)
            return false;

        var defOffset = address;

        if (defOffset >= 0x80000000 && defOffset < 0x80200000)
            defOffset -= 0x80000000;
        else
            return false;

        realAddress = (IntPtr)((ulong)emu_base + defOffset);
        return true;
    }

    internal override bool IsAddressInBounds<T>(ulong address)
    {
        return address + (ulong)Marshal.SizeOf(typeof(T)) <= 0x200000;
    }

    internal override bool IsStringAddressInBounds(ulong address, int stringLength)
    {
        return address + (ulong)stringLength <= 0x200000;
    }
}