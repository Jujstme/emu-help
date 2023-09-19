using System;

namespace LiveSplit.EMUHELP.Genesis
{
    internal abstract class GenesisBase : EmuBase
    {
        public IntPtr ram_base { get; protected set; } = default;
        public Endianness.Endian Endian { get; protected set; }

        internal GenesisBase(HelperBase helper)
            : base(helper) { }
    }
}