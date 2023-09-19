using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.PS1
{
    internal class Xebra : PS1Base
    {
        public Xebra(HelperBase helper) : base(helper)
        {
            var addr = Helper.game.SafeSigScanOrThrow(new SigScanTarget(1, "E8 ???????? E9 ???????? 89 C8 C1 F8 10")
            { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) });

            ram_base = Helper.game.ReadPointer(Helper.game.ReadPointer(addr + 0x16A)); // new DeepPointer(addr + 0x16A, 0).Deref<IntPtr>(_helper.game);
            ram_base.ThrowIfZero();

            Debugs.Info("  => Hooked to emulator: Xebra");
            Debugs.Info($"  => WRAM address found at 0x{ram_base.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            return true;
        }
    }
}