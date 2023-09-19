using LiveSplit.ComponentUtil;
using System;

namespace LiveSplit.EMUHELP.GBA
{
    internal class VisualBoyAdvance : GBABase
    {
        private readonly IntPtr cached_ewram_pointer;
        private readonly IntPtr cached_iwram_pointer;
        private readonly IntPtr is_emulating;

        public VisualBoyAdvance(HelperBase helper) : base(helper)
        {
            if (Helper.game.Is64Bit())
            {
                cached_ewram_pointer = Helper.game.SafeSigScanOrThrow(new SigScanTarget(3, "48 8B 05 ?? ?? ?? ?? 81 E3 FF FF 03 00")
                { OnFound = (p, s, addr) => { var ptr = addr + 0x4 + p.ReadValue<int>(addr); if (p.ReadValue<byte>(addr + 10) == 0x48) { ptr = p.ReadPointer(ptr); ptr.ThrowIfZero(); } return ptr; }});

                cached_iwram_pointer = Helper.game.SafeSigScanOrThrow(new SigScanTarget(3, "48 8B 05 ?? ?? ?? ?? 81 E3 FF 7F 00 00")
                { OnFound = (p, s, addr) => { var ptr = addr + 0x4 + p.ReadValue<int>(addr); if (p.ReadValue<byte>(addr + 10) == 0x48) { ptr = p.ReadPointer(ptr); ptr.ThrowIfZero(); } return ptr; }});

                SigScanTarget[] isEmulating =
                {
                    new SigScanTarget(2, "83 3D ?? ?? ?? ?? 00 74 ?? 80 3D ?? ?? ?? ?? 00 75 ?? 66") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) + 0x1 },
                    new SigScanTarget(3, "48 8B 15 ?? ?? ?? ?? 31 C0 8B 12 85 D2 74 ?? 48") { OnFound = (p, s, addr) => p.ReadPointer(addr + 0x4 + p.ReadValue<int>(addr)) },
                };

                foreach (var entry in isEmulating)
                {
                    is_emulating = Helper.game.SafeSigScan(entry);
                    if (!is_emulating.IsZero())
                        break;
                }
                is_emulating.ThrowIfZero();
            }
            else
            {
                cached_ewram_pointer = Helper.game.SafeSigScan(new SigScanTarget(1, "A1 ?? ?? ?? ?? 81 ?? FF FF 03 00") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

                if (!cached_ewram_pointer.IsZero())
                {
                    cached_iwram_pointer = Helper.game.SafeSigScanOrThrow(new SigScanTarget(1, "A1 ?? ?? ?? ?? 81 ?? FF 7F 00 00") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

                    SigScanTarget[] isEmulating =
                    {
                        new SigScanTarget(2, "83 3D ?? ?? ?? ?? 00 74 ?? 80 3D ?? ?? ?? ?? 00 75 ?? 66") { OnFound = (p, s, addr) => p.ReadPointer(addr) },
                        new SigScanTarget(2, "8B 15 ?? ?? ?? ?? 31 C0 85 D2 74 ?? 0F") { OnFound = (p, s, addr) => p.ReadPointer(addr) },
                    };

                    foreach (var entry in isEmulating)
                    {
                        is_emulating = Helper.game.SafeSigScan(entry);
                        if (!is_emulating.IsZero())
                            break;
                    }
                    is_emulating.ThrowIfZero();
                }
                else
                {
                    cached_ewram_pointer = Helper.game.SafeSigScanOrThrow(new SigScanTarget(8, "81 E6 FF FF 03 00 8B 15") { OnFound = (p, s, addr) => p.ReadPointer(addr) });
                    cached_iwram_pointer = cached_ewram_pointer + 0x4;
                    is_emulating = Helper.game.SafeSigScanOrThrow(new SigScanTarget(2, "8B 0D ?? ?? ?? ?? 85 C9 74 ?? 8A") { OnFound = (p, s, addr) => p.ReadPointer(addr) });
                }
            }

            ewram = Helper.game.ReadPointer(cached_ewram_pointer);
            iwram = Helper.game.ReadPointer(cached_iwram_pointer);

            Debugs.Info("  => Hooked to emulator: VisualBoyAdvance");

            if (!ewram.IsZero())
                Debugs.Info($"  => EWRAM address found at 0x{ewram.ToString("X")}");

            if (!iwram.IsZero())
                Debugs.Info($"  => IWRAM address found at 0x{iwram.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            if (Helper.game.ReadValue<bool>(is_emulating, out var isok))
            {
                ewram = isok ? Helper.game.ReadPointer(cached_ewram_pointer) : IntPtr.Zero;
                iwram = isok ? Helper.game.ReadPointer(cached_iwram_pointer) : IntPtr.Zero;
                return true;
            } else
                return false;
        }
    }
}