using System;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.GBA
{
    internal class Mednafen : GBABase
    {
        private readonly IntPtr ewram_pointer;
        private readonly IntPtr iwram_pointer;

        internal Mednafen(HelperBase helper) : base(helper)
        {
            ewram_pointer = Helper.Game.Is64Bit()
                ? Helper.Game.SafeSigScanOrThrow(new SigScanTarget(3, "48 8B 05 ?? ?? ?? ?? 81 E1 FF FF 03 00")
                { OnFound = (p, s, addr) => { var ptr = addr + 0x4 + p.ReadValue<int>(addr); if (p.ReadValue<byte>(addr + 10) == 0x48) { ptr = p.ReadPointer(ptr); ptr.ThrowIfZero(); } return ptr; } })
                : Helper.Game.SafeSigScanOrThrow(new SigScanTarget(1, "A1 ?? ?? ?? ?? 81 ?? FF FF 03 00") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

            iwram_pointer = Helper.Game.Is64Bit()
                ? Helper.Game.SafeSigScanOrThrow(new SigScanTarget(3, "48 8B 05 ?? ?? ?? ?? 81 E1 FF 7F 00 00")
                { OnFound = (p, s, addr) => { var ptr = addr + 0x4 + p.ReadValue<int>(addr); if (p.ReadValue<byte>(addr + 10) == 0x48) { ptr = p.ReadPointer(ptr); ptr.ThrowIfZero(); } return ptr; } })
                : Helper.Game.SafeSigScanOrThrow(new SigScanTarget(1, "A1 ?? ?? ?? ?? 81 ?? FF 7F 00 00") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

            ewram = Helper.Game.ReadPointer(ewram_pointer);
            iwram = Helper.Game.ReadPointer(iwram_pointer);

            Debugs.Info("  => Hooked to emulator: Mednafen");
            Debugs.Info($"  => EWRAM address found at 0x{ewram.ToString("X")}");
            Debugs.Info($"  => IWRAM address found at 0x{iwram.ToString("X")}");
        }

        internal override bool KeepAlive()
        {
            ewram = Helper.Game.ReadPointer(ewram_pointer);
            iwram = Helper.Game.ReadPointer(iwram_pointer);
            return true;
        }
    }
}