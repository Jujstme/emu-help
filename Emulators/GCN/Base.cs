using System;

namespace LiveSplit.EMUHELP.GCN
{
    internal abstract class GCNBase : EmuBase
    {
        internal IntPtr MEM1 { get; set; } = default;

        internal GCNBase(HelperBase helper)
            : base(helper)
        {
            Endian = Endianness.Endian.Big;
        }

        internal override IntPtr GetMemoryAddress(int region) => MEM1;
    }
}