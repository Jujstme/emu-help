using System;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.GBC
{
    internal class GSR : GBCBase
    {
        private readonly IntPtr base_wram_addr;
        private readonly IntPtr base_iohram_addr;

        internal GSR(HelperBase helper) : base(helper)
        {
            var game = Helper.Game;

            if (!game.Is64Bit())
                throw new Exception();

            base_wram_addr = game.GetSymbols(game.MainModuleWow64Safe()).FirstOrDefault(s => s.Name == "GSR_GB_WRAM_PTR").Address;
            base_iohram_addr = game.GetSymbols(game.MainModuleWow64Safe()).FirstOrDefault(s => s.Name == "GSR_GB_HRAM_PTR").Address;

            if (base_wram_addr.IsZero() || base_iohram_addr.IsZero())
                throw new Exception();

            wram_base = game.ReadPointer(base_wram_addr);
            iohram_base = game.ReadPointer(base_iohram_addr) - 0x80;

            Debugs.Info("  => Hooked to emulator: GSR");

            if (wram_base != IntPtr.Zero && iohram_base != IntPtr.Zero)
            {
                Debugs.Info($"  => WRAM address found at 0x{wram_base.ToString("X")}");
                Debugs.Info($"  => IO_HRAM address found at 0x{iohram_base.ToString("X")}");
            }
        }

        internal override bool KeepAlive()
        {
            if (Helper.Game.ReadPointer(base_wram_addr, out var _wram_base) && Helper.Game.ReadPointer(base_iohram_addr, out var _iohram_base))
            {
                wram_base = _wram_base;
                iohram_base = _iohram_base - 0x80;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
