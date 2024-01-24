using System;

namespace LiveSplit.EMUHELP.SMS
{
    internal abstract class SMSBase : EmuBase
    {
        internal IntPtr ram_base { get; set; } = default;

        internal SMSBase(HelperBase helper)
            : base(helper) { }

        internal override IntPtr GetMemoryAddress(int _) => ram_base;
    }
}