using System;

namespace LiveSplit.EMUHELP.WII
{
    internal abstract class WIIBase : EmuBase
    {
        internal IntPtr MEM1 { get; set; } = default;
        internal IntPtr MEM2 { get; set; } = default;

        internal WIIBase(HelperBase helper)
            : base(helper)
        {
            Endian = Endianness.Endian.Big;
        }

        internal override IntPtr GetMemoryAddress(int region) => region switch
        {
            0 => MEM1,
            _ => MEM2,
        };
    }
}