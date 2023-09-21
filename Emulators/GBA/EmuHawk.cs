using LiveSplit.ComponentUtil;
using System;
using System.Linq;

namespace LiveSplit.EMUHELP.GBA
{
    internal class EmuHawk : GBABase
    {
        private readonly IntPtr core_base;

        public EmuHawk(HelperBase helper) : base(helper)
        {
            core_base = Helper.game.ModulesWow64Safe().First(m => m.ModuleName == "mgba.dll").BaseAddress;

            ewram = Helper.game
                .MemoryPages(true)
                .First(p => (int)p.RegionSize == 0x48000 && (p.AllocationProtect & MemPageProtect.PAGE_READWRITE) != 0)
                .BaseAddress;

            iwram = ewram + 0x40000;

            Debugs.Info("  => Hooked to emulator: BizHawk");
            Debugs.Info($"  => EWRAM address found at 0x{ewram.ToString("X")}");
            Debugs.Info($"  => IWRAM address found at 0x{iwram.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            return Helper.game.ReadBytes(core_base, 1, out _) && Helper.game.ReadBytes(ewram, 1, out _);
        }
    }
}