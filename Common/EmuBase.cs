using System;

namespace LiveSplit.EMUHELP
{
    internal abstract class EmuBase
    {
        internal HelperBase Helper { get; set; }
        internal abstract bool KeepAlive();
        internal abstract IntPtr GetMemoryAddress(int region);
        internal Endianness.Endian Endian { get; set; } = Endianness.Endian.Little;

        internal EmuBase(HelperBase helper)
        {
            Helper = helper;
            GC.Collect();
        }
    }
}
