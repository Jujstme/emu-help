using System;

namespace LiveSplit.EMUHELP.WII
{
    internal abstract class WIIBase : EmuBase
    {
        public IntPtr MEM1 { get; protected set; } = default;
        public IntPtr MEM2 { get; protected set; } = default;
        public Endianness.Endian Endian { get; protected set; } = Endianness.Endian.Big;

        internal WIIBase(HelperBase helper)
            : base(helper) { }
    }
}