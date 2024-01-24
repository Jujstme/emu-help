using LiveSplit.ComponentUtil;
using System;

namespace LiveSplit.EMUHELP.GBA
{
    internal class NoCashGBA : GBABase
    {
        private readonly IntPtr base_addr;
        const int ewram_offset = 0x938C + 0x8;
        const int iwram_offset = 0x95D4;

        internal NoCashGBA(HelperBase helper) : base(helper)
        {
            base_addr = Helper.Game.SafeSigScanOrThrow(new SigScanTarget(2, "FF 35 ?? ?? ?? ?? 55") { OnFound = (p, s, addr) => p.ReadPointer(addr) });
            IntPtr addr = Helper.Game.ReadPointer(base_addr);
            ewram = Helper.Game.ReadPointer(addr + ewram_offset);
            iwram = Helper.Game.ReadPointer(addr + iwram_offset);

            Debugs.Info("  => Hooked to emulator: NO$GBA");
            Debugs.Info($"  => EWRAM address found at 0x{ewram.ToString("X")}");
            Debugs.Info($"  => IWRAM address found at 0x{iwram.ToString("X")}");
        }

        internal override bool KeepAlive()
        {
            if (Helper.Game.ReadPointer(base_addr, out var addr))
            {
                ewram = Helper.Game.ReadPointer(addr + ewram_offset);
                iwram = Helper.Game.ReadPointer(addr + iwram_offset);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}