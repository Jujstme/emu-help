using System;

namespace LiveSplit.EMUHELP.GBA
{
    internal abstract class GBABase : EmuBase
    {
        public IntPtr ewram { get; set; } = default;
        public IntPtr iwram { get; set; } = default;

        internal GBABase(HelperBase helper)
            : base(helper) { }
    }
}