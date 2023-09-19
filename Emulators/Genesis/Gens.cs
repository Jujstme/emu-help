using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.Genesis
{
    internal class Gens : GenesisBase
    {
        public Gens(HelperBase helper) : base(helper)
        {
            var wr_base = Helper.game.SafeSigScanOrThrow(new SigScanTarget(11, "72 ?? 81 ?? FF FF 00 00 66 8B"));

            Endian = Helper.game.ReadValue<byte>(wr_base + 4) == 0x86
                ? Endianness.Endian.Big
                : Endianness.Endian.Little;

            ram_base = Helper.game.ReadPointer(wr_base);
            ram_base.ThrowIfZero();

            Debugs.Info("  => Hooked to emulator: Fusion");
            Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            return true;
        }
    }
}