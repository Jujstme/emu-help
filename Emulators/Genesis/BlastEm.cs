using System;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class Genesis
{
    private Tuple<IntPtr, Func<bool>> BlastEm()
    {
        Endianess = Endianess.LittleEndian;

        IntPtr WRAMbase = IntPtr.Zero;
        foreach (var page in game.MemoryPages().Where(p => (int)p.RegionSize == 0x101000))
        {
            WRAMbase = new SignatureScanner(game, page.BaseAddress, (int)page.RegionSize)
                .Scan(new SigScanTarget(11, "72 0E 81 E1 FF FF 00 00 66 8B 89 ???????? C3") { OnFound = (p, s, addr) => p.ReadPointer(addr) });
            
            if (!WRAMbase.IsZero())
                break;
        }

        WRAMbase.ThrowIfZero();

        Debugs.Info("  => Hooked to emulator: BlastEm");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, () => true);
    }
}