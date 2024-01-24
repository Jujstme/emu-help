using System;

namespace LiveSplit.EMUHELP.PS1
{
    internal abstract class PS1Base : EmuBase
    {
        internal IntPtr ram_base = default;

        internal PS1Base(HelperBase helper)
            : base(helper) { }

        internal override IntPtr GetMemoryAddress(int index) => ram_base;
    }
}