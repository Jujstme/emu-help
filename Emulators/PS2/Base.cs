using System;

namespace LiveSplit.EMUHELP.PS2
{
    internal abstract class PS2Base : EmuBase
    {
        internal IntPtr ram_base { get; set; } = default;

        internal PS2Base(HelperBase helper)
            : base(helper) { }

        internal override IntPtr GetMemoryAddress(int _) => ram_base;
    }
}