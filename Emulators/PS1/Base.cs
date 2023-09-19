using System;

namespace LiveSplit.EMUHELP.PS1
{
    internal abstract class PS1Base : EmuBase
    {
        public IntPtr ram_base { get; set; } = default;

        internal PS1Base(HelperBase helper)
            : base(helper) { }
    }
}