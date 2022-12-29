using System;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS1
{
    public Tuple<IntPtr, Func<bool>> EmuHawk()
    {
        IntPtr WRAMbase = game.MemoryPages(true).FirstOrDefault(p => p.Type == MemPageType.MEM_MAPPED && (int)p.RegionSize == 0xD819000).BaseAddress;
        LiveSplit.EMUHELP.ExtensionMethods.CheckPtr(WRAMbase);
        WRAMbase += 0x2FADC0;

        bool checkIfAlive() => game.ReadBytes(WRAMbase, 1, out _);

        Debugs.Info("  => Hooked to emulator: EmuHawk / BizHawk");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, (Func<bool>)checkIfAlive);
    }
}
