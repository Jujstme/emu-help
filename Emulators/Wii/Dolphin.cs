using LiveSplit.ComponentUtil;
using System.Linq;

namespace LiveSplit.EMUHELP.WII
{
    internal class Dolphin : WIIBase
    {
        public Dolphin(HelperBase helper) : base(helper)
        {
            Endian = Endianness.Endian.Big;

            MEM1 = Helper.game.MemoryPages(true)
                .First(p => p.Type == MemPageType.MEM_MAPPED && (int)p.RegionSize == 0x2000000
                    && Helper.game.ReadValue<long>(p.BaseAddress + 0x3118) == 0x0000000400000004)
                .BaseAddress;

            MEM2 = Helper.game.MemoryPages(true)
                .First(p => p.Type == MemPageType.MEM_MAPPED && (int)p.RegionSize == 0x4000000
                    && (long)p.BaseAddress > (long)MEM1 && (long)p.BaseAddress < (long)MEM1 + 0x10000000)
                .BaseAddress;

            Debugs.Info("  => Hooked to emulator: Dolphin");

            if (!MEM1.IsZero())
                Debugs.Info($"  => MEM1 address found at 0x{MEM1.ToString("X")}");
            if (!MEM2.IsZero())
                Debugs.Info($"  => MEM2 address found at 0x{MEM2.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            return Helper.game.ReadBytes(MEM1, 1, out _) && Helper.game.ReadBytes(MEM2, 1, out _);
        }
    }
}