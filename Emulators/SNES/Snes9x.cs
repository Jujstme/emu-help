using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class SNES
{
    private Tuple<IntPtr, Func<bool>> Snes9x()
    {
        IntPtr Base = game.Is64Bit()
            ? game.SigScanner().ScanOrThrow(new SigScanTarget(4, "49 8B 94 C0 ???????? F7 C1") { OnFound = (p, s, addr) => p.MainModuleWow64Safe().BaseAddress + p.ReadValue<int>(addr) })
            : game.SigScanner().ScanOrThrow(new SigScanTarget(1, "E8 ???????? A2 ???????? 0F B6") { OnFound = (p, s, addr) => p.ReadPointer(addr + 0x4 + p.ReadValue<int>(addr) + 0x17) });

        IntPtr WRAMbase = game.ReadPointer(Base);
        WRAMbase.ThrowIfZero();

        // Probably not required
        // bool keepAlive() => game.ReadPointer(Base) == WRAMbase;

        Debugs.Info("  => Hooked to emulator: Snes9x");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, () => true);
    }
}