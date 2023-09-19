using System;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.PS1
{
    internal class Duckstation : PS1Base
    {
        private readonly IntPtr base_addr;

        public Duckstation(HelperBase helper) : base(helper)
        {
            var game = Helper.game;

            if (!game.Is64Bit())
                throw new Exception();

            base_addr = game.SafeSigScanOrThrow(new SigScanTarget(3, "48 89 0D ???????? B8") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) });
            ram_base = game.ReadPointer(base_addr);

            Debugs.Info("  => Hooked to emulator: Duckstation");

            if (ram_base != IntPtr.Zero)
                Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            var success = Helper.game.ReadPointer(base_addr, out var addr);

            if (success)
            {
                ram_base = addr;
                return true;
            } else
                return false;
        }
    }
}