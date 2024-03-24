using System;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.GBC
{
    internal class GSR_qt : GBCBase
    {
        private readonly IntPtr base_addr;
        private readonly IntPtr base_wram_addr;

        internal GSR_qt(HelperBase helper) : base(helper)
        {
            var game = Helper.Game;
            foreach (var page in game.MemoryPages().Where(p => (int)p.RegionSize == 0xFF000))
            {
                base_addr = new SignatureScanner(Helper.Game, page.BaseAddress, (int)page.RegionSize)
                    .Scan(new SigScanTarget(0, "20 ?? ?? ?? 20 ?? ?? ?? 20 ?? ?? ?? 20 ?? ?? ?? 05 00 00"));

                if (!base_addr.IsZero())
                {
                    break;
                }
            }
            if (base_addr.IsZero())
                Debugs.Info("  =>base_addr still zero...");

            base_addr.ThrowIfZero();

            Debugs.Info("  => Hooked into emulator: GSR_qt");
            base_wram_addr = base_addr - 0x10;
            wram_base = game.ReadPointer(base_wram_addr);
            iohram_base = base_addr + 0x13FC;

            if (wram_base != IntPtr.Zero && iohram_base != IntPtr.Zero)
            {
                Debugs.Info($"  => WRAM Address found at 0x{wram_base.ToString("X")}");
                Debugs.Info($"  => IO_HRAM Address found at 0x{iohram_base.ToString("X")}");
                Debugs.Info($"  => WRAM Pointer: 0x{(base_wram_addr).ToString("X")}");
            }
        }


        internal override bool KeepAlive()
        {
            var success = Helper.Game.ReadPointer(base_addr, out var addr);

            if (success)
            {
                wram_base = Helper.Game.ReadPointer(addr - 0x10);
                iohram_base = addr + 0x13FC;
                return true;
            }
            else
                return false;
        }
    }
}
