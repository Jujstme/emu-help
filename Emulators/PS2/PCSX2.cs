using LiveSplit.ComponentUtil;
using System;

namespace LiveSplit.EMUHELP.PS2
{
    internal class PCSX2 : PS2Base
    {
        private readonly bool is64Bit;
        private readonly IntPtr addr_base;

        public PCSX2(HelperBase helper) : base(helper)
        {
            is64Bit = Helper.game.Is64Bit();

            if (is64Bit)
            {
                addr_base = Helper.game.SafeSigScanOrThrow(new SigScanTarget(3, "48 8B ?? ?? ?? ?? ?? 25 F0 3F 00 00") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) });
            }
            else
            {
                SigScanTarget[] targets =
                {
                    new SigScanTarget(2, "8B ?? ?? ?? ?? ?? 25 F0 3F 00 00") { OnFound = (p, s, addr) => p.ReadPointer(addr) },
                    new SigScanTarget(2, "8B ?? ?? ?? ?? ?? 81 ?? F0 3F 00 00") { OnFound = (p, s, addr) => p.ReadPointer(addr) },
                };

                foreach (var entry in targets)
                {
                    addr_base = Helper.game.SafeSigScan(entry);
                    if (!addr_base.IsZero())
                        break;
                }

                addr_base.ThrowIfZero();
            }

            ram_base = Helper.game.ReadPointer(addr_base);

            Debugs.Info("  => Hooked to emulator: PCSX2");
            Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            if (Helper.game.ReadPointer(addr_base, is64Bit, out var ptr))
            {
                ram_base = ptr;
                return true;
            }
            else
                return false;
        }
    }
}