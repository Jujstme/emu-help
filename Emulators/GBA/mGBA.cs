using LiveSplit.ComponentUtil;
using System.Linq;

namespace LiveSplit.EMUHELP.GBA
{
    internal class mGBA : GBABase
    {
        public mGBA(HelperBase helper) : base(helper)
        {
            ewram = Helper.game
                .MemoryPages(true)
                .First(p => (int)p.RegionSize == 0x48000 && (p.AllocationProtect & MemPageProtect.PAGE_READWRITE) != 0)
                .BaseAddress;

            iwram = ewram + 0x40000;

            Debugs.Info("  => Hooked to emulator: mGBA");
            Debugs.Info($"  => EWRAM address found at 0x{ewram.ToString("X")}");
            Debugs.Info($"  => IWRAM address found at 0x{iwram.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            return Helper.game.ReadBytes(ewram, 1, out _);
        }
    }
}