using System;

namespace LiveSplit.EMUHELP.GCN
{
    internal abstract class GCNBase : EmuBase
    {
        public IntPtr MEM1 { get; protected set; } = default;
        public Endianness.Endian Endian { get; protected set; } = Endianness.Endian.Big;

        internal GCNBase(HelperBase helper)
            : base(helper) { }
    }
}