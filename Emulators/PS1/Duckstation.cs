using System;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS1
{
    public Tuple<IntPtr, Func<bool>> Duckstation()
    {
        IntPtr WRAMbase = game.MemoryPages(true).FirstOrDefault(p => p.Type == MemPageType.MEM_MAPPED && (int)p.RegionSize == 0x200000).BaseAddress;
        WRAMbase.ThrowIfZero();

        bool checkIfAlive() => game.ReadBytes(WRAMbase, 1, out _);

        Debugs.Info("  => Hooked to emulator: Duckstation");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, checkIfAlive);
    }
}
