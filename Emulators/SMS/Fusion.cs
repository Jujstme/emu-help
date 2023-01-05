using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class SMS
{
    private Tuple<IntPtr, Func<bool>> Fusion()
    {
        IntPtr Base = new SignatureScanner(game, game.MainModuleWow64Safe().BaseAddress, game.MainModuleWow64Safe().ModuleMemorySize)
            .ScanOrThrow(new SigScanTarget(4, "74 C8 83 3D") { OnFound = (p, s, addr) => p.ReadPointer(addr) });


        IntPtr WRAMbase = game.ReadPointer(Base);
        WRAMbase.ThrowIfZero();

        bool keepAlive() => game.ReadPointer(Base) == WRAMbase;

        Debugs.Info("  => Hooked to emulator: Fusion");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase + 0xC000, keepAlive);
    }
}