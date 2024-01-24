using LiveSplit.ComponentUtil;
using System;
using System.Linq;

namespace LiveSplit.EMUHELP.Genesis
{
    internal class BlastEm : GenesisBase
    {
        internal BlastEm(HelperBase helper) : base(helper)
        {
            Endian = Endianness.Endian.Little;
            ram_base = IntPtr.Zero;

            foreach (var entry in Helper.Game.MemoryPages(true).Where(p => (int)p.RegionSize == 0x10100 && (p.AllocationProtect & MemPageProtect.PAGE_READWRITE) != 0))
            {
                ram_base = new SignatureScanner(Helper.Game, entry.BaseAddress, (int)entry.RegionSize).Scan(new SigScanTarget(11, "72 0E 81 E1 FF FF 00 00 66 8B 89 ?? ?? ?? ?? C3") { OnFound = (p, s, addr) => p.ReadPointer(addr) });
                
                if (!ram_base.IsZero())
                    break;
            }

            ram_base.ThrowIfZero();
            
            Debugs.Info("  => Hooked to emulator: SEGA Classics / SEGA Game Room");
            Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        internal override bool KeepAlive()
        {
            return true;
        }
    }
}