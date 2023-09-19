using LiveSplit.ComponentUtil;
using System;

namespace LiveSplit.EMUHELP.Genesis
{
    internal class Fusion : GenesisBase
    {
        private readonly IntPtr addr_base;

        public Fusion(HelperBase helper) : base(helper)
        {
            Endian = Endianness.Endian.Big;

            var ptr = Helper.game.SafeSigScanOrThrow(new SigScanTarget(1, "75 2F 6A 01"));
            ptr += Helper.game.ReadValue<byte>(ptr + 3);

            addr_base = Helper.game.ReadPointer(ptr);
            ram_base = Helper.game.ReadPointer(addr_base);

            Debugs.Info("  => Hooked to emulator: Fusion");
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