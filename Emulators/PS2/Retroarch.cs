using LiveSplit.ComponentUtil;
using System;
using System.Linq;

namespace LiveSplit.EMUHELP.PS2
{
    internal class Retroarch : PS2Base
    {
        private readonly bool is64Bit;
        private readonly IntPtr core_base_address;

        public Retroarch(HelperBase helper) : base(helper)
        {
            is64Bit = Helper.game.Is64Bit();

            if (!is64Bit)
                throw new Exception();

            string[] supportedCores =
            {
                "pcsx2_libretro.dll",
            };
            ProcessModuleWow64Safe currentCore = Helper.game.ModulesWow64Safe().First(m => supportedCores.Any(e => e == m.ModuleName));
            core_base_address = currentCore.BaseAddress;

            SignatureScanner scanner = new(Helper.game, core_base_address, currentCore.ModuleMemorySize);

            ram_base = scanner.ScanOrThrow(new SigScanTarget(3, "48 8B ?? ?? ?? ?? ?? 81 ?? F0 3F 00 00") { OnFound = (p, s, addr) => p.ReadPointer(addr + 0x4 + p.ReadValue<int>(addr)) });
            ram_base.ThrowIfZero();

            Debugs.Info("  => Hooked to emulator: PCSX2");
            Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            return Helper.game.ReadBytes(core_base_address, 1, out _);
        }
    }
}