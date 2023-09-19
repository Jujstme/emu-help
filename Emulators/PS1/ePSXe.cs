using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.PS1
{
    internal class ePSXe : PS1Base
    {
        public ePSXe(HelperBase helper) : base(helper)
        {
            ram_base = Helper.game.SafeSigScanOrThrow(new SigScanTarget(5, "C1 E1 10 8D 89") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

            Debugs.Info("  => Hooked to emulator: ePSXe");
            Debugs.Info($"  => WRAM address found at 0x{ram_base.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            return true;
        }
    }
}