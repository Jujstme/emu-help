using System;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.GBC
{
    internal class GSE : GBCBase
    {
        private readonly IntPtr base_wram_addr;
        private readonly IntPtr base_iohram_addr;

        internal GSE(HelperBase helper) : base(helper)
        {
            var game = Helper.Game;

            if (!game.Is64Bit())
                throw new Exception();

            string[] wram_symbol_name = { "GSE_GB_WRAM_PTR", "GSR_GB_WRAM_PTR" };
            string[] iohram_symbol_name = { "GSE_GB_HRAM_PTR", "GSR_GB_HRAM_PTR" };

            base_wram_addr = game.GetSymbols(game.MainModuleWow64Safe()).FirstOrDefault(s => wram_symbol_name.Contains(s.Name)).Address;
            base_iohram_addr = game.GetSymbols(game.MainModuleWow64Safe()).FirstOrDefault(s => iohram_symbol_name.Contains(s.Name)).Address;

            if (base_wram_addr.IsZero() || base_iohram_addr.IsZero())
                throw new Exception();

            wram_base = game.ReadPointer(base_wram_addr);
            iohram_base = game.ReadPointer(base_iohram_addr) - 0x80;

            Debugs.Info("  => Hooked to emulator: GSE");

            if (wram_base != IntPtr.Zero && iohram_base != IntPtr.Zero)
            {
                Debugs.Info($"  => WRAM address found at 0x{wram_base.ToString("X")}");
                Debugs.Info($"  => IO_HRAM address found at 0x{iohram_base.ToString("X")}");
            }
        }

        internal override bool KeepAlive()
        {
            if (!Helper.Game.ReadPointer(base_wram_addr, out var _wram_base)
                || !Helper.Game.ReadPointer(base_iohram_addr, out var _iohram_base))
                return false;
            
            wram_base = _wram_base;
            iohram_base = _iohram_base - 0x80;
            return true;
        }
    }
}
