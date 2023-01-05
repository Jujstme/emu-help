using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS1
{
    public Tuple<IntPtr, Func<bool>> ePSXe()
    {
        IntPtr WRAMbase = game.SigScan().ScanOrThrow(new SigScanTarget(5, "C1 E1 10 8D 89") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

        Debugs.Info("  => Hooked to emulator: ePSXe");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, () => true);
    }
}
