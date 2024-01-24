using System;

namespace LiveSplit.EMUHELP.Genesis
{
    internal abstract class GenesisBase : EmuBase
    {
        internal IntPtr ram_base { get; set; } = default;

        internal GenesisBase(HelperBase helper)
            : base(helper) { }

        internal override IntPtr GetMemoryAddress(int _) => ram_base;
    }
}