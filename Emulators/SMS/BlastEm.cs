using LiveSplit.ComponentUtil;
using System.Linq;

namespace LiveSplit.EMUHELP.SMS
{
    internal class BlastEm : SMSBase
    {
        public BlastEm(HelperBase helper) : base(helper)
        {
            foreach (var page in Helper.game.MemoryPages().Where(p => (int)p.RegionSize == 0x101000))
            {
                ram_base = new SignatureScanner(Helper.game, page.BaseAddress, (int)page.RegionSize)
                    .Scan(new SigScanTarget(10, "66 81 E1 FF 1F 0F B7 C9 8A 89 ???????? C3") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

                if (!ram_base.IsZero())
                    break;
            }

            ram_base.ThrowIfZero();

            Debugs.Info("  => Hooked to emulator: BlastEm");
            Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            return true;
        }
    }
}