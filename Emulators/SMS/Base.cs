using System;

namespace LiveSplit.EMUHELP.SMS
{
    internal abstract class SMSBase : EmuBase
    {
        public IntPtr ram_base { get; set; } = default;

        internal SMSBase(HelperBase helper)
            : base(helper) { }
    }
}