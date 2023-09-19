using LiveSplit.ComponentUtil;
using System;
using System.Linq;

namespace LiveSplit.EMUHELP.SMS
{
    internal class Retroarch : SMSBase
    {
        private readonly IntPtr core_base_address;

        public Retroarch(HelperBase helper) : base(helper)
        {
            string[] supportedCores =
            {
                "genesis_plus_gx_libretro.dll",
                "genesis_plus_gx_wide_libretro.dll",
                "picodrive_libretro.dll",
                "smsplus_libretro.dll",
                "gearsystem_libretro.dll",
            };

            ProcessModuleWow64Safe currentCore = Helper.game.ModulesWow64Safe().First(m => supportedCores.Any(e => e == m.ModuleName));
            core_base_address = currentCore.BaseAddress;

            SignatureScanner scanner = new(Helper.game, core_base_address, currentCore.ModuleMemorySize);


            ram_base = currentCore.ModuleName switch
            {
                "genesis_plus_gx_libretro.dll" or "genesis_plus_gx_wide_libretro.dll" => Helper.game.Is64Bit()
                    ? scanner.ScanOrThrow(new SigScanTarget(3, "48 8D 0D ???????? 4C 8B 2D ????????") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) })
                    : scanner.ScanOrThrow(new SigScanTarget(1, "A3 ???????? 29 F9") { OnFound = (p, s, addr) => p.ReadPointer(addr) }),
                "picodrive_libretro.dll" => (Helper.game.Is64Bit()
                    ? scanner.ScanOrThrow(new SigScanTarget(3, "48 8D 0D ???????? 41 B8 ????????") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) })
                    : scanner.ScanOrThrow(new SigScanTarget(1, "B9 ???????? C1 EF 10") { OnFound = (p, s, addr) => p.ReadPointer(addr) })) + 0x20000,
                "smsplus_libretro.dll" => Helper.game.Is64Bit()
                    ? scanner.ScanOrThrow(new SigScanTarget(5, "31 F6 48 C7 05") { OnFound = (p, s, addr) => addr + 0x8 + p.ReadValue<int>(addr) })
                    : scanner.ScanOrThrow(new SigScanTarget(4, "83 FA 02 B8") { OnFound = (p, s, addr) => p.ReadPointer(addr) }),
                "gearsystem_libretro.dll" => Helper.game.Is64Bit()
                    ? scanner.ScanOrThrow(new SigScanTarget(8, "83 ?? 02 75 ?? 48 8B 0D ?? ?? ?? ?? E8") { OnFound = (p, s, addr) => {
                        byte offset = p.ReadValue<byte>(addr + 13 + 0x4 + p.ReadValue<int>(addr + 13) + 0x3);
                        IntPtr ptr = p.ReadPointer(p.ReadPointer(p.ReadPointer(addr + 0x4 + p.ReadValue<int>(addr)) + 0x0) + offset); ptr.ThrowIfZero(); return ptr + 0xC000; } })
                    : scanner.ScanOrThrow(new SigScanTarget(7, "83 ?? 02 75 ?? 8B ?? ?? ?? ?? ?? E8") { OnFound = (p, s, addr) => { var ptr = p.ReadPointer(p.ReadPointer(p.ReadPointer(p.ReadPointer(addr)) + 0x0) + 0xC); ptr.ThrowIfZero(); return ptr + 0xC000; } }),
                _ => throw new NotImplementedException(),
            };

            Debugs.Info("  => Hooked to emulator: Retroarch");
            Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            return Helper.game.ReadBytes(core_base_address, 1, out _);
        }
    }
}