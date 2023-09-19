using LiveSplit.ComponentUtil;
using System;
using System.Linq;

namespace LiveSplit.EMUHELP.Genesis
{
    internal class SegaClassics : GenesisBase
    {
        private readonly IntPtr addr_base;

        public SegaClassics(HelperBase helper) : base(helper)
        {
            Endian = Endianness.Endian.Little;

            var GenesisWrapperModule = Helper
                .game
                .ModulesWow64Safe()
                .FirstOrDefault(m => m.ModuleName == "GenesisEmuWrapper.dll");

            addr_base = GenesisWrapperModule != null
                ? Helper.game.SigScanner(GenesisWrapperModule).ScanOrThrow(new SigScanTarget(2, "C7 05 ???????? ???????? A3 ???????? A3") { OnFound = (p, s, addr) => p.ReadPointer(addr) })
                : Helper.game.SafeSigScanOrThrow(new SigScanTarget(8, "89 2D ???????? 89 0D") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

            ram_base = Helper.game.ReadPointer(addr_base);

            Debugs.Info("  => Hooked to emulator: SEGA Classics / SEGA Game Room");

            if (!ram_base.IsZero())
                Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            if (Helper.game.ReadPointer(addr_base, out var addr))
            {
                ram_base = addr;
                return true;
            }
            else
                return false;
        }
    }
}