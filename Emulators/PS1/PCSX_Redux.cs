using System;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS1
{
    private Tuple<IntPtr, Func<bool>> PCSX_Redux()
    {
        int pageCount;

        IntPtr WRAMbase = game.MemoryPages(true).LastOrDefault(p => (int)p.RegionSize == 0x80100).BaseAddress;
        WRAMbase.ThrowIfZero();
        WRAMbase += game.Is64Bit() ? 0x40 : 0x20;
        pageCount = game.MemoryPages(true).Count();

        bool checkIfAlive() => game.MemoryPages(true).Count() == pageCount;

        Debugs.Info("  => Hooked to emulator: PCSX-Redux");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, checkIfAlive);
    }
}