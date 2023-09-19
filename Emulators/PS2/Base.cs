using System;

namespace LiveSplit.EMUHELP.PS2
{
    internal abstract class PS2Base : EmuBase
    {
        public IntPtr ram_base { get; set; } = default;

        internal PS2Base(HelperBase helper)
            : base(helper) { }
    }
}