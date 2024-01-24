using LiveSplit.ComponentUtil;
using System.Linq;

namespace LiveSplit.EMUHELP.GCN
{
    internal class Dolphin : GCNBase
    {
        internal Dolphin(HelperBase helper) : base(helper)
        {
            Endian = Endianness.Endian.Big;

            MEM1 = Helper.Game.MemoryPages(true)
                .First(p => p.Type == MemPageType.MEM_MAPPED && (int)p.RegionSize == 0x2000000
                    && Helper.Game.ReadValue<uint>(p.BaseAddress + 0x1C) == 0x3D9F33C2)
                .BaseAddress;

            Debugs.Info("  => Hooked to emulator: Dolphin");

            if (!MEM1.IsZero())
                Debugs.Info($"  => MEM1 address found at 0x{MEM1.ToString("X")}");
        }

        internal override bool KeepAlive()
        {
            return Helper.Game.ReadBytes(MEM1, 1, out _);
        }
    }
}