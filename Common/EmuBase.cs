using System;

namespace LiveSplit.EMUHELP
{
    internal abstract class EmuBase
    {
        internal HelperBase Helper { get; set; }
        public abstract bool KeepAlive();

        internal EmuBase(HelperBase helper)
        {
            Helper = helper;
            GC.Collect();
        }
    }
}
