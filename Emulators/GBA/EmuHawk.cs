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

            var mGBA = new mGBA(Helper);
            ewram = mGBA.ewram;
            iwram = mGBA.iwram;

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