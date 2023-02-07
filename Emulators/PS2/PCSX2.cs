using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS2
{
    private Tuple<IntPtr, Func<bool>> PCSX2()
    {
        var scanner = new SignatureScanner(game, game.MainModuleWow64Safe().BaseAddress, game.MainModuleWow64Safe().ModuleMemorySize);
        IntPtr Base = game.Is64Bit()
            ? scanner.ScanOrThrow(new SigScanTarget(4, "48 8B 8C C2 ???????? 48 85 C9") { OnFound = (p, s, addr) => game.MainModuleWow64Safe().BaseAddress + p.ReadValue<int>(addr) + 0x24B * 8 })
            : scanner.ScanOrThrow(new SigScanTarget(3, "8B 04 85 ???????? 85 C0 78 10") { OnFound = (p, s, addr) => p.ReadPointer(addr) + 0x24B * 4 });

        Base.ThrowIfZero();
        IntPtr WRAMbase = game.ReadPointer(Base);
        WRAMbase.ThrowIfZero();

        bool keepAlive() => game.ReadPointer(Base) == WRAMbase;

        Debugs.Info("  => Hooked to emulator: PCSX2");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, keepAlive);
    }
}