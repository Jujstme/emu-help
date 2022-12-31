using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS2
{
    public Tuple<IntPtr, Func<bool>> PCSX2()
    {
        SignatureScanner scanner = new SignatureScanner(game, game.MainModuleWow64Safe().BaseAddress, game.MainModuleWow64Safe().ModuleMemorySize);
        IntPtr WRAMbase = WRAMbase = game.Is64Bit()
            ? scanner.ScanOrThrow(new SigScanTarget(4, "48 8B 8C C2 ???????? 48 85 C9") { OnFound = (p, s, addr) => game.MainModuleWow64Safe().BaseAddress + p.ReadValue<int>(addr) + 0x5E3 * 8 })
            : scanner.ScanOrThrow(new SigScanTarget(3, "8B 04 85 ???????? 85 C0 78 10") { OnFound = (p, s, addr) => p.ReadPointer(addr) + 0x5E3 * 4 });

        Func<bool> keepAlive = () => !game.ReadPointer(WRAMbase).IsZero();

        WRAMbase = game.ReadPointer(WRAMbase);
        LiveSplit.EMUHELP.ExtensionMethods.ThrowIfZero(WRAMbase);

        Debugs.Info("  => Hooked to emulator: PCSX2");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, (Func<bool>)(() => true));
    }
}
