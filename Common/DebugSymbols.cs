using System;
using System.Runtime.InteropServices;

namespace LiveSplit.EMUHELP
{
    public struct Symbol
    {
        public readonly IntPtr Address;
        public readonly string Name;

        public Symbol(IntPtr address, string name)
        {
            this.Address = address;
            this.Name = name;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct ExportedSymbolsTableDef
        {
            [FieldOffset(0x14)] public int numberOfFunctions;
            [FieldOffset(0x1C)] public int functionAddressArrayIndex;
            [FieldOffset(0x20)] public int functionNameArrayIndex;
        }
    }
}
