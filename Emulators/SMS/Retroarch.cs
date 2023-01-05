using System;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class SMS
{
    public Tuple<IntPtr, Func<bool>> Retroarch()
    {
        var supportedCores = new string[]
        {
            "genesis_plus_gx_libretro.dll",
            "genesis_plus_gx_wide_libretro.dll",
            "picodrive_libretro.dll",
            // SMS plus GX
            // Gearsystem
        };

        game.ResetModulesWow64Cache();
        ProcessModuleWow64Safe CurrentCore = game.ModulesWow64Safe().First(m => supportedCores.Contains(m.ModuleName));

        IntPtr WRAMbase = IntPtr.Zero;
        SignatureScanner scanner;

        switch (CurrentCore.ModuleName.ToLower())
        {
            case "genesis_plus_gx_libretro.dll":
            case "genesis_plus_gx_wide_libretro.dll":
                scanner = game.SigScan(CurrentCore);
                WRAMbase = game.Is64Bit()
                    ? scanner.ScanOrThrow(new SigScanTarget(3, "48 8D 0D ???????? 4C 8B 2D ????????") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) })
                    : scanner.ScanOrThrow(new SigScanTarget(1, "A3 ???????? 29 F9") { OnFound = (p, s, addr) => p.ReadPointer(addr) });
                break;

            case "picodrive_libretro.dll":
                scanner = game.SigScan(CurrentCore);
                WRAMbase = game.Is64Bit()
                    ? scanner.ScanOrThrow(new SigScanTarget(3, "48 8D 0D ???????? 41 B8 ????????") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) })
                    : scanner.ScanOrThrow(new SigScanTarget(1, "B9 ???????? C1 EF 10") { OnFound = (p, s, addr) => p.ReadPointer(addr) });
                WRAMbase += 0x20000;
                break;

            default:
                throw new Exception();
        }

        bool checkIfAlive() => game.ReadBytes(CurrentCore.BaseAddress, 1, out _);


        Debugs.Info("  => Hooked to emulator: Retroarch");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, checkIfAlive);
    }
}
