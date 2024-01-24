using System;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.PS1
{
    internal class Duckstation : PS1Base
    {
        private readonly IntPtr base_addr;

        internal Duckstation(HelperBase helper) : base(helper)
        {
            var game = Helper.Game;

            if (!game.Is64Bit())
                throw new Exception();

            base_addr = game.GetSymbols(game.MainModuleWow64Safe()).FirstOrDefault(s => s.Name == "RAM").Address;

            if (base_addr.IsZero())
                base_addr = game.SafeSigScanOrThrow(new SigScanTarget(3, "48 89 0D ???????? B8") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) });

            ram_base = game.ReadPointer(base_addr);

            Debugs.Info("  => Hooked to emulator: Duckstation");

            if (ram_base != IntPtr.Zero)
                Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        internal override bool KeepAlive()
        {
            var success = Helper.Game.ReadPointer(base_addr, out var addr);

            if (success)
            {
                ram_base = addr;
                return true;
            } else
                return false;
        }
    }
}