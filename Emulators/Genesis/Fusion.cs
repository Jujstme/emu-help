using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class Genesis
{
    private Tuple<IntPtr, Func<bool>> Fusion()
    {
        Endianess = Endianess.BigEndian;

        var Base = new SignatureScanner(game, game.MainModuleWow64Safe().BaseAddress, game.MainModuleWow64Safe().ModuleMemorySize)
            .ScanOrThrow(new SigScanTarget(1, "75 2F 6A 01") { OnFound = (p, s, addr) => p.ReadPointer(addr + p.ReadValue<byte>(addr) + 3) });

        
        IntPtr WRAMbase = game.ReadPointer(Base);
        WRAMbase.ThrowIfZero();

        bool keepAlive() => game.ReadPointer(Base) == WRAMbase;
        
        Debugs.Info("  => Hooked to emulator: Fusion");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, keepAlive);
    }
}