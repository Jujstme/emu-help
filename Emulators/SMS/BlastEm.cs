using System;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class SMS
{
    private Tuple<IntPtr, Func<bool>> BlastEm()
    {
        IntPtr WRAMbase = IntPtr.Zero;

        foreach (var page in game.MemoryPages().Where(p => (int)p.RegionSize == 0x101000))
        {
            WRAMbase = new SignatureScanner(game, page.BaseAddress, (int)page.RegionSize)
                .Scan(new SigScanTarget(10, "66 81 E1 FF 1F 0F B7 C9 8A 89 ???????? C3") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

            if (!WRAMbase.IsZero())
                break;
        }

        WRAMbase.ThrowIfZero();

        Debugs.Info("  => Hooked to emulator: BlastEm");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, () => true);
    }
}