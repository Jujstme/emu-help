using System;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.GBA
{
    internal class Mednafen : GBABase
    {
        private readonly IntPtr ewram_pointer;
        private readonly IntPtr iwram_pointer;

        public Mednafen(HelperBase helper) : base(helper)
        {
            ewram_pointer = Helper.game.Is64Bit()
                ? Helper.game.SafeSigScanOrThrow(new SigScanTarget(3, "48 8B 05 ?? ?? ?? ?? 81 E1 FF FF 03 00")
                { OnFound = (p, s, addr) => { var ptr = addr + 0x4 + p.ReadValue<int>(addr); if (p.ReadValue<byte>(addr + 10) == 0x48) { ptr = p.ReadPointer(ptr); ptr.ThrowIfZero(); } return ptr; } })
                : Helper.game.SafeSigScanOrThrow(new SigScanTarget(1, "A1 ?? ?? ?? ?? 81 ?? FF FF 03 00") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

            iwram_pointer = Helper.game.Is64Bit()
                ? Helper.game.SafeSigScanOrThrow(new SigScanTarget(3, "48 8B 05 ?? ?? ?? ?? 81 E1 FF 7F 00 00")
                { OnFound = (p, s, addr) => { var ptr = addr + 0x4 + p.ReadValue<int>(addr); if (p.ReadValue<byte>(addr + 10) == 0x48) { ptr = p.ReadPointer(ptr); ptr.ThrowIfZero(); } return ptr; } })
                : Helper.game.SafeSigScanOrThrow(new SigScanTarget(1, "A1 ?? ?? ?? ?? 81 ?? FF 7F 00 00") { OnFound = (p, s, addr) => p.ReadPointer(addr) });

            ewram = Helper.game.ReadPointer(ewram_pointer);
            iwram = Helper.game.ReadPointer(iwram_pointer);

            Debugs.Info("  => Hooked to emulator: Mednafen");
            Debugs.Info($"  => EWRAM address found at 0x{ewram.ToString("X")}");
            Debugs.Info($"  => IWRAM address found at 0x{iwram.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            ewram = Helper.game.ReadPointer(ewram_pointer);
            iwram = Helper.game.ReadPointer(iwram_pointer);
            return true;
        }
    }
}