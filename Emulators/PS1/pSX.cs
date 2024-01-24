using System;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.PS1
{
    internal class pSX : PS1Base
    {
        internal pSX(HelperBase helper) : base(helper)
        {
            var game = Helper.Game;
            var scanner = game.SigScanner();

            SigScanTarget[] scan =
            {
                new(2, "8B 15 ?? ?? ?? ?? 8D 34 1A") { OnFound = (p, s, addr) => p.ReadPointer(addr) },
                new(1, "A1 ?? ?? ?? ?? 8D 34 18") { OnFound = (p, s, addr) => p.ReadPointer(addr) },
                new(1, "A1 ?? ?? ?? ?? 8B 7C 24 14") { OnFound = (p, s, addr) => p.ReadPointer(addr) },
                new(1, "A1 ?? ?? ?? ?? 8B 6C 24") { OnFound = (p, s, addr) => p.ReadPointer(addr) },
            };

            var ptr = IntPtr.Zero;

            foreach (var entry in scan)
            {
                ptr = game.SafeSigScan(entry);

                if (!ptr.IsZero())
                    break;
            }

            ptr.ThrowIfZero();

            ram_base = game.ReadPointer(ptr);
            ram_base.ThrowIfZero();

            Debugs.Info("  => Hooked to emulator: pSX");
            Debugs.Info($"  => WRAM address found at 0x{ram_base.ToString("X")}");
        }

        internal override bool KeepAlive() => true;
    }
}