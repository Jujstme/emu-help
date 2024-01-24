using LiveSplit.ComponentUtil;
using System;

namespace LiveSplit.EMUHELP.SMS
{
    internal class Fusion : SMSBase
    {
        private readonly IntPtr base_addr;

        internal Fusion(HelperBase helper) : base(helper)
        {
            base_addr = Helper.Game.SafeSigScanOrThrow(new SigScanTarget(4, "74 C8 83 3D") { OnFound = (p, s, addr) => p.ReadPointer(addr) });
            ram_base = Helper.Game.ReadPointer(base_addr);
            ram_base.ThrowIfZero();
            ram_base += 0xC000;

            Debugs.Info("  => Hooked to emulator: Fusion");
            Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        internal override bool KeepAlive()
        {
            if (Helper.Game.ReadPointer(base_addr, out var addr))
            {
                ram_base = addr;
                return true;
            }
            else
                return false;
        }
    }
}