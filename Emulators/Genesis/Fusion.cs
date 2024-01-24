using LiveSplit.ComponentUtil;
using System;

namespace LiveSplit.EMUHELP.Genesis
{
    internal class Fusion : GenesisBase
    {
        private readonly IntPtr addr_base;

        internal Fusion(HelperBase helper) : base(helper)
        {
            Endian = Endianness.Endian.Big;

            var ptr = Helper.Game.SafeSigScanOrThrow(new SigScanTarget(1, "75 2F 6A 01"));
            ptr += Helper.Game.ReadValue<byte>(ptr + 3);

            addr_base = Helper.Game.ReadPointer(ptr);
            ram_base = Helper.Game.ReadPointer(addr_base);

            Debugs.Info("  => Hooked to emulator: Fusion");
            Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        internal override bool KeepAlive()
        {
            if (Helper.Game.ReadPointer(addr_base, out var addr))
            {
                ram_base = addr;
                return true;
            }
            else
                return false;
        }
    }
}