using System;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.PS1
{
    internal class Retroarch : PS1Base
    {
        private readonly IntPtr core_base_address;

        public Retroarch(HelperBase helper) : base(helper)
        {
            var game = Helper.game;

            string[] supportedCores =
            {
                "mednafen_psx_hw_libretro.dll",
                "mednafen_psx_libretro.dll",
                "swanstation_libretro.dll",
                "pcsx_rearmed_libretro.dll",
            };

            ProcessModuleWow64Safe currentCore = game.ModulesWow64Safe().First(m => supportedCores.Any(e => e == m.ModuleName));
            core_base_address = currentCore.BaseAddress;

            bool is64Bit = game.Is64Bit();
            SignatureScanner scanner = new(game, core_base_address, currentCore.ModuleMemorySize);

            ram_base = game.Is64Bit() switch
            {
                true => currentCore.ModuleName switch
                {
                    "mednafen_psx_hw_libretro.dll" or "mednafen_psx_libretro.dll" => scanner.ScanOrThrow(new SigScanTarget(3, "48 8B 05 ?? ?? ?? ?? 41 81 E4 FF FF 1F 00") { OnFound = (p, s, addr) => p.ReadPointer(addr + 0x4 + p.ReadValue<int>(addr)) }),
                    "swanstation_libretro.dll" => scanner.ScanOrThrow(new SigScanTarget(3, "48 89 0D ?? ?? ?? ?? 89 35 ?? ?? ?? ?? 89 3D") { OnFound = (p, s, addr) => p.ReadPointer(addr + 0x4 + p.ReadValue<int>(addr)) }),
                    "pcsx_rearmed_libretro.dll" => scanner.ScanOrThrow(new SigScanTarget(3, "48 8B 35 ?? ?? ?? ?? 81 E2") { OnFound = (p, s, addr) => p.ReadPointer(addr + 0x4 + p.ReadValue<int>(addr)) }),
                    _ => throw new NotImplementedException(),
                },
                false => currentCore.ModuleName switch
                {
                    "mednafen_psx_hw_libretro.dll" or "mednafen_psx_libretro.dll" => scanner.ScanOrThrow(new SigScanTarget(1, "A1 ?? ?? ?? ?? 81 E3 FF FF 1F 00") { OnFound = (p, s, addr) => p.ReadPointer(p.ReadPointer(addr)) }),
                    "swanstation_libretro.dll" => scanner.ScanOrThrow(new SigScanTarget(1, "A1 ?? ?? ?? ?? 23 CB 8B") { OnFound = (p, s, addr) => p.ReadPointer(p.ReadPointer(addr)) }),
                    "pcsx_rearmed_libretro.dll" => scanner.ScanOrThrow(new SigScanTarget(9, "FF FF 1F 00 89 ?? ?? ?? A1") { OnFound = (p, s, addr) => p.ReadPointer(p.ReadPointer(addr)) }),
                    _ => throw new NotImplementedException(),
                },
            };

            ram_base.ThrowIfZero();

            Debugs.Info("  => Hooked to emulator: Retroarch");
            Debugs.Info($"  => WRAM address found at 0x{ram_base.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            return Helper.game.ReadBytes(core_base_address, 1, out _);
        }
    }
}