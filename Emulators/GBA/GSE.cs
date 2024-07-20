using LiveSplit.ComponentUtil;
using System.Linq;
using System;

namespace LiveSplit.EMUHELP.GBA
{
    internal class GSE : GBABase
    {
        private readonly IntPtr ewram_pointer;
        private readonly IntPtr iwram_pointer;
        internal GSE(HelperBase helper) : base(helper)
        {
            var game = Helper.Game;

            string[] ewram_symbol_name = { "GSE_GBA_EWRAM_PTR", "GSR_GBA_EWRAM_PTR" };
            string[] iwram_symbol_name = { "GSE_GBA_IWRAM_PTR", "GSR_GBA_IWRAM_PTR" };

            ewram_pointer = game.GetSymbols(game.MainModuleWow64Safe()).FirstOrDefault(s => ewram_symbol_name.Contains(s.Name)).Address;
            iwram_pointer = game.GetSymbols(game.MainModuleWow64Safe()).FirstOrDefault(s => iwram_symbol_name.Contains(s.Name)).Address;

            if (ewram_pointer.IsZero() || iwram_pointer.IsZero())
                throw new Exception();

            ewram = game.ReadPointer(ewram_pointer);
            iwram = game.ReadPointer(iwram_pointer);

            Debugs.Info("  => Hooked to emulator: GSE");
            Debugs.Info($"  => EWRAM address found at 0x{ewram.ToString("X")}");
            Debugs.Info($"  => IWRAM address found at 0x{iwram.ToString("X")}");
        }

        internal override bool KeepAlive()
        {
            if (!Helper.Game.ReadPointer(ewram_pointer, out var ewram_addr)
                || !Helper.Game.ReadPointer(iwram_pointer, out var iwram_addr))
                return false;
            
            ewram = ewram_addr;
            iwram = iwram_addr;
            return true;
        }
    }
}
