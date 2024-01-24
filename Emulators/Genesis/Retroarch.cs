using System;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.Genesis
{
    internal class Retroarch : GenesisBase
    {
        private readonly IntPtr core_base_address;

        internal Retroarch(HelperBase helper) : base(helper)
        {
            var game = Helper.Game;

            string[] supportedCores =
            {
                "blastem_libretro.dll",
                "genesis_plus_gx_libretro.dll",
                "genesis_plus_gx_wide_libretro.dll",
                "picodrive_libretro.dll",
            };

            ProcessModuleWow64Safe currentCore = game.ModulesWow64Safe().First(m => supportedCores.Any(e => e == m.ModuleName));
            core_base_address = currentCore.BaseAddress;

            bool is64Bit = game.Is64Bit();
            var scanner = new SignatureScanner(game, core_base_address, currentCore.ModuleMemorySize);

            Endian = Endianness.Endian.Little;

            if (currentCore.ModuleName == "blastem_libretro.dll")
            {
                foreach (var entry in Helper.Game.MemoryPages(true).Where(p => (int)p.RegionSize == 0x10100 && (p.AllocationProtect & MemPageProtect.PAGE_READWRITE) != 0))
                {
                    ram_base = new SignatureScanner(Helper.Game, entry.BaseAddress, (int)entry.RegionSize).Scan(new SigScanTarget(11, "72 0E 81 E1 FF FF 00 00 66 8B 89 ?? ?? ?? ?? C3") { OnFound = (p, s, addr) => (IntPtr)p.ReadValue<int>(addr) });

                    if (!ram_base.IsZero())
                        break;
                }
            }
            else if (currentCore.ModuleName == "genesis_plus_gx_libretro.dll" || currentCore.ModuleName == "genesis_plus_gx_wide_libretro.dll")
            {
                ram_base = Helper.Game.Is64Bit() switch
                {
                    true => scanner.ScanOrThrow(new SigScanTarget(3, "48 8D 0D ?? ?? ?? ?? 4C 8B 2D") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) }),
                    false => scanner.ScanOrThrow(new SigScanTarget(1, "A3 ?? ?? ?? ?? 29 F9") { OnFound = (p, s, addr) => p.ReadPointer(addr) }),
                };
            }
            else if (currentCore.ModuleName == "picodrive_libretro.dll")
            {
                ram_base = Helper.Game.Is64Bit() switch
                {
                    true => scanner.ScanOrThrow(new SigScanTarget(3, "48 8D 0D ?? ?? ?? ?? 41 B8") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) }),
                    false => scanner.ScanOrThrow(new SigScanTarget(1, "B9 ?? ?? ?? ?? C1 EF 10") { OnFound = (p, s, addr) => p.ReadPointer(addr) }),
                };
            }
            else
                throw new NotImplementedException();

            ram_base.ThrowIfZero();

            Debugs.Info("  => Hooked to emulator: Retroarch");
            Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        internal override bool KeepAlive() => Helper.Game.ReadBytes(core_base_address, 1, out _);
    }
}