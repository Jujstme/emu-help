using System;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS1
{
    public Tuple<IntPtr, Func<bool>> pSX()
    {
        IntPtr WRAMbase = game.MemoryPages(true).FirstOrDefault(p => p.Type == MemPageType.MEM_PRIVATE && (int)p.RegionSize == 0x201000).BaseAddress;
        WRAMbase.ThrowIfZero();
        WRAMbase += 0x20;

        bool checkIfAlive() => true;

        Debugs.Info("  => Hooked to emulator: pSX");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, checkIfAlive);
    }
}
