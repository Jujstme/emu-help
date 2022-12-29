using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS1
{
    public Tuple<IntPtr, Func<bool>> Xebra()
    {
        IntPtr WRAMbase = new SignatureScanner(game, game.MainModuleWow64Safe().BaseAddress, game.MainModuleWow64Safe().ModuleMemorySize)
            .ScanOrThrow(new SigScanTarget(1, "E8 ???????? E9 ???????? 89 C8 C1 F8 10") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) });

        WRAMbase = new DeepPointer(WRAMbase + 0x16A, 0).Deref<IntPtr>(game);

        Debugs.Info("  => Hooked to emulator: Xebra");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, (Func<bool>)(() => true));
    }
}
