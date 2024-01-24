using System;

namespace LiveSplit.EMUHELP.GBA
{
    internal abstract class GBABase : EmuBase
    {
        internal IntPtr ewram { get; set; } = default;
        internal IntPtr iwram { get; set; } = default;

        internal GBABase(HelperBase helper)
            : base(helper) { }

        internal override IntPtr GetMemoryAddress(int region)
        {
            return region switch
            {
                0 => ewram,
                _ => iwram,
            };
        }
    }
}