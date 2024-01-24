using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP.Genesis
{
    internal class Gens : GenesisBase
    {
        internal Gens(HelperBase helper) : base(helper)
        {
            var wr_base = Helper.Game.SafeSigScanOrThrow(new SigScanTarget(11, "72 ?? 81 ?? FF FF 00 00 66 8B"));

            Endian = Helper.Game.ReadValue<byte>(wr_base + 4) == 0x86
                ? Endianness.Endian.Big
                : Endianness.Endian.Little;

            ram_base = Helper.Game.ReadPointer(wr_base);
            ram_base.ThrowIfZero();

            Debugs.Info("  => Hooked to emulator: Fusion");
            Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        internal override bool KeepAlive() => true;
    }
}