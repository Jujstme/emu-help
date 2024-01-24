using LiveSplit.ComponentUtil;
using System;

namespace LiveSplit.EMUHELP.PS1
{
    internal class Mednafen : PS1Base
    {
        internal Mednafen(HelperBase helper) : base(helper)
        {
            ram_base = Helper.Game.SafeSigScanOrThrow(Helper.Game.Is64Bit()
                ? new SigScanTarget(5, "89 01 0F B6 82") { OnFound = (p, s, addr) => (IntPtr)p.ReadValue<int>(addr) }
                : new SigScanTarget(5, "89 01 0F B6 82 ?? ?? ?? ?? C3") { OnFound = (p, s, addr) => (IntPtr)p.ReadValue<int>(addr) });

            Debugs.Info("  => Hooked to emulator: Mednafen");
            Debugs.Info($"  => WRAM address found at 0x{ram_base.ToString("X")}");
        }

        internal override bool KeepAlive()
        {
            return true;
        }
    }
}