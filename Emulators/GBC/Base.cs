using System;

namespace LiveSplit.EMUHELP.GBC
{
    internal abstract class GBCBase : EmuBase
    {
        internal IntPtr wram_base { get; set; } = default;
        internal IntPtr iohram_base { get; set; } = default;
        internal GBCBase(HelperBase helper)
            : base(helper)
        {
            Endian = Endianness.Endian.Big;
        }

        internal override IntPtr GetMemoryAddress(int region)
        {
            return region switch
            {
                0 => wram_base,
                1 => iohram_base,
                _ => wram_base
            };
        }
    }
}
