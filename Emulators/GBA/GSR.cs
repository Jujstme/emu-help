using LiveSplit.ComponentUtil;
using System.Linq;
using System;

namespace LiveSplit.EMUHELP.GBA
{
    internal class GSR : GBABase
    {
        private readonly IntPtr ewram_pointer;
        private readonly IntPtr iwram_pointer;
        internal GSR(HelperBase helper) : base(helper)
        {
            var game = Helper.Game;
            ewram_pointer = game.GetSymbols(game.MainModuleWow64Safe()).FirstOrDefault(s => s.Name == "GSR_GBA_EWRAM_PTR").Address;
            iwram_pointer = game.GetSymbols(game.MainModuleWow64Safe()).FirstOrDefault(s => s.Name == "GSR_GBA_IWRAM_PTR").Address;

            if (ewram_pointer.IsZero() || iwram_pointer.IsZero())
            {
                Debugs.Info("  => Pointers are 0.");
                throw new Exception();
            }
            ewram = game.ReadPointer(ewram_pointer);
            iwram = game.ReadPointer(iwram_pointer);

            Debugs.Info("  => Hooked to emulator: GSR");
            Debugs.Info($"  => EWRAM address found at 0x{ewram.ToString("X")}");
            Debugs.Info($"  => IWRAM address found at 0x{iwram.ToString("X")}");
        }

        internal override bool KeepAlive()
        {
            var success = Helper.Game.ReadPointer(ewram_pointer, out var ewram_addr);

            if (success)
            {
                iwram = Helper.Game.ReadPointer(iwram_pointer);
                ewram = ewram_addr;
                return true;
            } else
                return false;
        }
    }
}