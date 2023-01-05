using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class Genesis
{
    private Tuple<IntPtr, Func<bool>> Gens()
    {
        Endianess = Endianess.LittleEndian;

        var scanner = new SignatureScanner(game, game.MainModuleWow64Safe().BaseAddress, game.MainModuleWow64Safe().ModuleMemorySize);
        IntPtr WRAMbase = scanner.ScanOrThrow(new SigScanTarget(11, "72 ?? 81 ?? FF FF 00 00 66 8B ?? ????????"));

        if (game.ReadValue<byte>(WRAMbase + 4) == 0x86)
            Endianess = Endianess.BigEndian;

        WRAMbase = game.ReadPointer(WRAMbase);

        Debugs.Info("  => Hooked to emulator: Gens");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, () => true);
    }
}