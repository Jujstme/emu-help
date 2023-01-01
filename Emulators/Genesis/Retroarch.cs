using System;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class Genesis
{
    public Tuple<IntPtr, Func<bool>> Retroarch()
    {
        var supportedCores = new string[]
        {
            "blastem_libretro.dll",
            "genesis_plus_gx_libretro.dll",
            "genesis_plus_gx_wide_libretro.dll",
            "picodrive_libretro.dll",
        };

        game.ResetModulesWow64Cache();
        ProcessModuleWow64Safe CurrentCore = game.ModulesWow64Safe().First(m => supportedCores.Contains(m.ModuleName));
      
        IntPtr WRAMbase = IntPtr.Zero;
        SignatureScanner scanner;

        switch (CurrentCore.ModuleName.ToLower())
        {
            case "blastem_libretro.dll":
                foreach (var page in game.MemoryPages().Where(p => (int)p.RegionSize == 0x101000))
                {
                    WRAMbase = new SignatureScanner(game, page.BaseAddress, (int)page.RegionSize)
                        .Scan(new SigScanTarget(11, "72 0E 81 E1 FF FF 00 00 66 8B 89 ???????? C3") { OnFound = (p, s, addr) => (IntPtr)p.ReadValue<int>(addr) });
                    if (!WRAMbase.IsZero())
                        break;
                }
                WRAMbase.ThrowIfZero();
                Endianess = Endianess.LittleEndian;
                break;

            case "genesis_plus_gx_libretro.dll":
            case "genesis_plus_gx_wide_libretro.dll":
                scanner = new SignatureScanner(game, CurrentCore.BaseAddress, CurrentCore.ModuleMemorySize);
                WRAMbase = game.Is64Bit()
                    ? scanner.ScanOrThrow(new SigScanTarget(3, "48 8D 0D ???????? 4C 8B 2D ????????") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) })
                    : scanner.ScanOrThrow(new SigScanTarget(1, "A3 ???????? 29 F9") { OnFound = (p, s, addr) => p.ReadPointer(addr) });
                Endianess = Endianess.LittleEndian;
                break;

            case "picodrive_libretro.dll":
                scanner = new SignatureScanner(game, CurrentCore.BaseAddress, CurrentCore.ModuleMemorySize);
                WRAMbase = game.Is64Bit()
                    ? scanner.ScanOrThrow(new SigScanTarget(3, "48 8D 0D ???????? 41 B8 ????????") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) })
                    : scanner.ScanOrThrow(new SigScanTarget(1, "B9 ???????? C1 EF 10") { OnFound = (p, s, addr) => p.ReadPointer(addr) });
                Endianess = Endianess.LittleEndian;
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
