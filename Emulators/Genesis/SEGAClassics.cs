using System;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class Genesis
{
    private Tuple<IntPtr, Func<bool>> SEGAClassics()
    {
        Endianess = Endianess.LittleEndian;

        IntPtr WRAMbase;

        if (game.ProcessName.ToLower() == "segagameroom")
        {
            var module = game.ModulesWow64Safe().First(m => m.ModuleName.ToLower() == "genesisemuwrapper.dll");
            WRAMbase = new SignatureScanner(game, module.BaseAddress, module.ModuleMemorySize)
                .ScanOrThrow(new SigScanTarget(2, "C7 05 ???????? ???????? A3 ???????? A3 ????????") { OnFound = (p, s, addr) => p.ReadPointer(p.ReadPointer(addr)) });
        } else
        {
            WRAMbase = new SignatureScanner(game, game.MainModuleWow64Safe().BaseAddress, game.MainModuleWow64Safe().ModuleMemorySize)
                        .ScanOrThrow(new SigScanTarget(8, "89 2D ???????? 89 0D ????????") { OnFound = (p, s, addr) => p.ReadPointer(p.ReadPointer(addr)) });
        }

        WRAMbase.ThrowIfZero();

        Debugs.Info("  => Hooked to emulator: SEGA Classics");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, () => true);
    }
}