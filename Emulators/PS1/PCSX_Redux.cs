﻿using System;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.PS1
{
    internal class PcsxRedux : PS1Base
    {
        private readonly bool is64Bit;
        private readonly IntPtr addr_base;
        private readonly IntPtr addr;

        internal PcsxRedux(HelperBase helper) : base(helper)
        {
            is64Bit = Helper.Game.Is64Bit();

            if (is64Bit)
            {
                var scanner = Helper.Game.SigScanner();

                addr_base = scanner.ScanOrThrow(new SigScanTarget(2, "48 B9 ?? ?? ?? ?? ?? ?? ?? ?? E8 ?? ?? ?? ?? C7 85 ?? ?? ?? ?? 00 00 00 00"));
                addr = (IntPtr)Helper.Game.ReadValue<long>(addr_base);
                addr.ThrowIfZero();

                var ptr = scanner.ScanOrThrow(new SigScanTarget(8, "89 D1 C1 E9 10 48 8B"));
                var offset = Helper.Game.ReadValue<byte>(ptr);

                ram_base = (IntPtr)new DeepPointer(this.addr + offset, 0).Deref<long>(Helper.Game);
            }
            else
            {
                var target = new SigScanTarget(2, "8B 3D 20 ?? ?? ?? 0F B7 D3 8B 04 95 ?? ?? ?? ?? 21 05");

                foreach (var entry in Helper.Game.MemoryPages(true))
                {
                    addr_base = new SignatureScanner(Helper.Game, entry.BaseAddress, (int)entry.RegionSize).Scan(target);
                    if (!addr_base.IsZero())
                        break;
                }
                addr_base.ThrowIfZero();

                addr = Helper.Game.ReadPointer(addr_base);
                ram_base = addr;
            }

            Debugs.Info("  => Hooked to emulator: PCSX-Redux");
            Debugs.Info($"  => WRAM address found at 0x{ram_base.ToString("X")}");
        }

        internal override bool KeepAlive() => Helper.Game.ReadPointer(addr_base, is64Bit, out var ptr) && ptr == addr;
    }
}