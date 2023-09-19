using System;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.PS1
{
    internal class PcsxRedux : PS1Base
    {
        private readonly bool is64Bit;
        private readonly IntPtr addr_base;
        private readonly IntPtr addr;

        public PcsxRedux(HelperBase helper) : base(helper)
        {
            is64Bit = Helper.game.Is64Bit();

            if (is64Bit)
            {
                var scanner = Helper.game.SigScanner();

                addr_base = scanner.ScanOrThrow(new SigScanTarget(2, "48 B9 ?? ?? ?? ?? ?? ?? ?? ?? E8 ?? ?? ?? ?? C7 85 ?? ?? ?? ?? 00 00 00 00"));
                addr = (IntPtr)Helper.game.ReadValue<long>(addr_base);
                addr.ThrowIfZero();

                var ptr = scanner.ScanOrThrow(new SigScanTarget(8, "89 D1 C1 E9 10 48 8B"));
                var offset = Helper.game.ReadValue<byte>(ptr);

                ram_base = (IntPtr)new DeepPointer(this.addr + offset, 0).Deref<long>(Helper.game);
            }
            else
            {
                var target = new SigScanTarget(2, "8B 3D 20 ?? ?? ?? 0F B7 D3 8B 04 95 ?? ?? ?? ?? 21 05");

                foreach (var entry in Helper.game.MemoryPages(true))
                {
                    addr_base = new SignatureScanner(Helper.game, entry.BaseAddress, (int)entry.RegionSize).Scan(target);
                    if (!addr_base.IsZero())
                        break;
                }
                addr_base.ThrowIfZero();

                addr = Helper.game.ReadPointer(addr_base);
                ram_base = addr;
            }

            Debugs.Info("  => Hooked to emulator: PCSX-Redux");
            Debugs.Info($"  => WRAM address found at 0x{ram_base.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            if (Helper.game.ReadPointer(addr_base, is64Bit, out var ptr))
                return ptr == addr;
            else
                return false;
        }
    }
}