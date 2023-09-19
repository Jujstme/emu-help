using System;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.GBA
{
    internal class Retroarch : GBABase
    {
        private readonly IntPtr core_base_address;

        public Retroarch(HelperBase helper) : base(helper)
        {
            var game = Helper.game;

            string[] supportedCores =
            {
                "vbam_libretro.dll",
                "mednafen_gba_libretro.dll",
                "vba_next_libretro.dll",
                "mgba_libretro.dll",
                "gpsp_libretro.dll",
            };

            ProcessModuleWow64Safe currentCore = game.ModulesWow64Safe().First(m => supportedCores.Any(e => e == m.ModuleName));
            core_base_address = currentCore.BaseAddress;

            bool is64Bit = game.Is64Bit();
            var scanner = new SignatureScanner(game, core_base_address, currentCore.ModuleMemorySize);

            IntPtr[] ram = currentCore.ModuleName switch
            {
                "vbam_libretro.dll" or "mednafen_gba_libretro.dll" or "vba_next_libretro.dll" => vba(currentCore, is64Bit),
                "mgba_libretro.dll" => mGBA(),
                "gpsp_libretro.dll" => gpSP(currentCore, is64Bit),
                _ => throw new NotImplementedException(),
            };

            ewram = ram[0];
            iwram = ram[1];

            ewram.ThrowIfZero();
            iwram.ThrowIfZero();

            Debugs.Info("  => Hooked to emulator: Retroarch");
            Debugs.Info($"  => EWRAM address found at 0x{ewram.ToString("X")}");
            Debugs.Info($"  => IWRAM address found at 0x{iwram.ToString("X")}");
        }

        private IntPtr[] vba(ProcessModuleWow64Safe currentCore, bool is64Bit)
        {
            SignatureScanner scanner = new(Helper.game, core_base_address, currentCore.ModuleMemorySize);

            IntPtr ewram_pointer = is64Bit
                ? scanner.ScanOrThrow(new SigScanTarget(3, "48 8B 05 ?? ?? ?? ?? 81 E1 FF FF 03 00")
                { OnFound = (p, s, addr) => { var ptr = addr + 0x4 + p.ReadValue<int>(addr); if (p.ReadValue<byte>(addr + 10) == 0x48) { ptr = p.ReadPointer(ptr); ptr.ThrowIfZero(); } return ptr; }})
                : scanner.ScanOrThrow(new SigScanTarget(1, "A1 ?? ?? ?? ?? 81 ?? FF FF 03 00") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

            IntPtr iwram_pointer = is64Bit
                ? scanner.ScanOrThrow(new SigScanTarget(3, "48 8B 05 ?? ?? ?? ?? 81 E1 FF 7F 00 00")
                { OnFound = (p, s, addr) => { var ptr = addr + 0x4 + p.ReadValue<int>(addr); if (p.ReadValue<byte>(addr + 10) == 0x48) { ptr = p.ReadPointer(ptr); ptr.ThrowIfZero(); } return ptr; }})
                : scanner.ScanOrThrow(new SigScanTarget(1, "A1 ?? ?? ?? ?? 81 ?? FF 7F 00 00") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

            IntPtr ewram = Helper.game.ReadPointer(ewram_pointer);
            IntPtr iwram = Helper.game.ReadPointer(iwram_pointer);
            return new IntPtr[] { ewram, iwram };
        }

        private IntPtr[] mGBA()
        {
            var gba = new mGBA(Helper);
            return new IntPtr[] { gba.ewram, gba.iwram };
        }

        private IntPtr[] gpSP(ProcessModuleWow64Safe currentCore, bool is64Bit)
        {
            var scanner = new SignatureScanner(Helper.game, core_base_address, currentCore.ModuleMemorySize);

            IntPtr base_addr = is64Bit
                ? scanner.ScanOrThrow(new SigScanTarget(3, "48 8B 15 ?? ?? ?? ?? 8B 42 40") { OnFound = (p, s, addr) => p.ReadPointer(addr + 0x4 + p.ReadValue<int>(addr)) })
                : scanner.ScanOrThrow(new SigScanTarget(1, "A3 ?? ?? ?? ?? F7 C5 02 00 00 00") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

            IntPtr ewram = base_addr + Helper.game.ReadValue<int>(scanner.ScanOrThrow(new SigScanTarget(8, "25 FF FF 03 00 88 94 03")));
            IntPtr iwram = base_addr + Helper.game.ReadValue<int>(scanner.ScanOrThrow(new SigScanTarget(9, "25 FE 7F 00 00 66 89 94 03")));
            return new IntPtr[] { ewram, iwram };
        }

        public override bool KeepAlive()
        {
            return Helper.game.ReadBytes(core_base_address, 1, out _);
        }
    }
}