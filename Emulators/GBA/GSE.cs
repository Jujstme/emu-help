﻿using LiveSplit.ComponentUtil;
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

            // Maintain backwards compatibility after rename from GSR -> GSE
            var ptr_prefix = helper.Game.ProcessName;

            ewram_pointer = game.GetSymbols(game.MainModuleWow64Safe()).FirstOrDefault(s => s.Name == ptr_prefix + "_GBA_EWRAM_PTR").Address;
            iwram_pointer = game.GetSymbols(game.MainModuleWow64Safe()).FirstOrDefault(s => s.Name == ptr_prefix + "_GBA_IWRAM_PTR").Address;

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
            if (Helper.Game.ReadPointer(ewram_pointer, out var ewram_addr) && Helper.Game.ReadPointer(iwram_pointer, out var iwram_addr))
            {
                ewram = ewram_addr;
                iwram = iwram_addr;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
